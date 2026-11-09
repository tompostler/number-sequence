function Write-Cyan([string]$statement) {
    Write-Host -ForegroundColor Cyan "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss.ff'))] $statement";
}
function Write-Yellow([string]$statement) {
    Write-Host -ForegroundColor Yellow "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss.ff'))] $statement";
}

$subscriptionId = '78560c44-50bb-4840-9d59-84578a99032e';
$tenantId = '7ba82f12-6fd5-4d98-b0bb-8ff879870903';

# Fetch a secret as a raw string.
# Deliberately uses json output instead of tsv, since tsv escapes embedded newlines and tabs, which would corrupt the secrets holding json blobs.
function Get-Secret([string]$name) {
    $value = az keyvault secret show --vault-name tompostler --name $name --query value --output json | ConvertFrom-Json;
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to read secret '$name' from key vault.";
    }
    return $value;
}

# Fetch a secret that the app can run without, such as a library lookup token not registered yet. A missing one is null,
# which the app treats as that feature being off.
function Get-OptionalSecret([string]$name) {
    $value = az keyvault secret show --vault-name tompostler --name $name --query value --output json 2>$null | ConvertFrom-Json;
    if ($LASTEXITCODE -ne 0) {
        Write-Yellow "No '$name' secret in key vault; leaving it unset.";
        return $null;
    }
    return $value;
}

Write-Cyan 'Logging in to subscription....';
# A cached login is not proof of a usable token, so probe a real token instead of just checking that an account exists.
# Conditional access is evaluated per resource, so arm and key vault are separate grants and have to be probed, and if needed logged into, separately.
az account get-access-token --resource 'https://management.azure.com' --output none 2>$null;
if ($LASTEXITCODE -ne 0) {
    az login --tenant $tenantId --output none;
    if ($LASTEXITCODE -ne 0) {
        throw 'az login failed for arm.';
    }
}
az account set --subscription $subscriptionId;
if ($LASTEXITCODE -ne 0) {
    throw "Could not select subscription $subscriptionId.";
}

az account get-access-token --resource 'https://vault.azure.net' --output none 2>$null;
if ($LASTEXITCODE -ne 0) {
    Write-Yellow 'Key vault needs its own consent, prompting again....';
    az login --tenant $tenantId --scope 'https://vault.azure.net/.default' --output none;
    if ($LASTEXITCODE -ne 0) {
        throw 'az login failed for key vault.';
    }
}
Write-Host;

# Note: the following needs to be kept up-to-date with any necessary config changes
Write-Cyan 'Generating local settings....';
$localSettings = [PSCustomObject]@{
    ApplicationInsights = [PSCustomObject]@{
        # Dummy value to satisfy the SDK parser locally; no telemetry is sent without a real key.
        ConnectionString = 'InstrumentationKey=00000000-0000-0000-0000-000000000000';
    };
    Claude              = [PSCustomObject]@{
        ApiKey = (Get-Secret 'claude-api-key');
    };
    Email               = [PSCustomObject]@{
        ChiroBatchMap      = (Get-Secret 'email-chiro-batch-map').Replace('\"', '"');
        ChiroBatchUri      = (Get-Secret 'email-chiro-batch-uri');
        Server             = (Get-Secret 'email-server');
        Port               = (Get-Secret 'email-port');
        Username           = (Get-Secret 'email-username');
        Password           = (Get-Secret 'email-password');
        LocalDevToOverride = (git config --get user.email);
    };
    LibraryLookup       = [PSCustomObject]@{
        BggToken  = (Get-OptionalSecret 'library-lookup-bgg-token');
        TmdbToken = (Get-OptionalSecret 'library-lookup-tmdb-token');
    };
    Sql                 = [PSCustomObject]@{
        ConnectionString = (
            'Server=tcp:tompostler.database.windows.net,1433;Initial Catalog=nslocal;Persist Security Info=False;' `
                + 'User ID=sqladmin;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Password=' `
                + (Get-Secret 'tompostler-sqladmin-password') `
                + ';');
    };
    Storage             = [PSCustomObject]@{
        ConnectionString = (
            'DefaultEndpointsProtocol=https;AccountName=nstcpwtflocal;AccountKey=' `
                + (az storage account keys list --resource-group tcp-wtf-hosting --account-name nstcpwtflocal --query '[1].value' --output tsv) `
                + ';EndpointSuffix=core.windows.net');
    };
};
$localSettingsPath = Join-Path ($PSScriptRoot) '.\src\number-sequence\appsettings.Development.json';
# Create the item (including path!) if it doesn't exist
New-Item -Path $localSettingsPath -ItemType File -Force | Out-Null;
$localSettings | ConvertTo-Json | Set-Content -Path $localSettingsPath;
Write-Host;

$confirm = Read-Host 'Do you wish to also replace the nslocal sql database and library covers from prod? Note, this may take several minutes. [yN]';
if ($confirm -eq 'y') {
    Write-Cyan 'Copying sql database and library covers from prod to localdev.';
    Write-Host;

    az sql db show --resource-group tompostler --server tompostler --name nslocal --output none 2>$null;
    if ($LASTEXITCODE -eq 0) {
        Write-Cyan 'Deleting existing nslocal sql database....';
        az sql db delete --resource-group tompostler --server tompostler --name nslocal --yes;
    }
    Write-Host;

    # Started without waiting so progress can be shown. The database's status is Copying until it's Online. Its
    # operations carry a percentComplete, which a copy may only report as 0 then 100, so elapsed time is always shown too.
    Write-Cyan 'Creating copy of production sql database to nslocal....';
    az sql db copy --resource-group tompostler --server tompostler --name ns --dest-resource-group tompostler --dest-server tompostler --dest-name nslocal --no-wait --output none;
    if ($LASTEXITCODE -ne 0) {
        throw 'Failed to start copying the sql database.';
    }
    $copyStarted = Get-Date;
    do {
        Start-Sleep -Seconds 7;
        $elapsed = (Get-Date) - $copyStarted;
        $status = az sql db show --resource-group tompostler --server tompostler --name nslocal --query status --output tsv 2>$null;
        $operation = az sql db op list --resource-group tompostler --server tompostler --database nslocal --output json 2>$null | ConvertFrom-Json `
            | Sort-Object -Property startTime -Descending | Select-Object -First 1;
        if ($operation.state -in 'Failed', 'Cancelled') {
            throw "Copying the sql database $($operation.state.ToLowerInvariant()): $($operation.errorDescription)";
        }
        if ($elapsed.TotalMinutes -gt 60) {
            throw 'Copying the sql database has taken over an hour; check it in the portal.';
        }
        $progress = if ($operation.percentComplete) { ", $($operation.percentComplete)% done" } else { '' };
        Write-Host ("  {0:00}:{1:00} elapsed, {2}{3}" -f [math]::Floor($elapsed.TotalMinutes), $elapsed.Seconds, $(if ($status) { $status } else { 'started' }), $progress);
    } while ($status -ne 'Online');
    Write-Host;

    Write-Cyan 'Resizing nslocal sql database to Basic....';
    az sql db update --resource-group tompostler --server tompostler --name nslocal --service-objective Basic --output none;
    Write-Host;

    # Library covers are flagged in sql (LibraryItem.HasCover) but stored in blob, so they're mirrored along with the
    # database, or every copied cover is a broken image locally. Every blob is overwritten, never skipped: a replaced cover
    # keeps its name, and a local-only item can share an id with a later prod one. Local-only blobs are left; nothing shows
    # a cover its item isn't flagged for. The pdf container isn't copied: those are patient documents, and nothing local
    # needs them.
    # start-batch only starts each copy; the storage service finishes them in the background, so the destination's copy
    # status is polled until none is pending.
    $prodStorageKey = az storage account keys list --resource-group tcp-wtf-hosting --account-name nstcpwtf --query '[1].value' --output tsv;
    $localStorageKey = az storage account keys list --resource-group tcp-wtf-hosting --account-name nstcpwtflocal --query '[1].value' --output tsv;
    $blobCount = [int](az storage blob list --account-name nstcpwtf --account-key $prodStorageKey --container-name library --num-results '*' --query 'length(@)' --output tsv);
    Write-Cyan "Copying $blobCount library blobs from nstcpwtf to nstcpwtflocal....";
    az storage blob copy start-batch --account-name nstcpwtflocal --account-key $localStorageKey --destination-container library `
        --source-account-name nstcpwtf --source-account-key $prodStorageKey --source-container library --output none;
    do {
        $copies = az storage blob list --account-name nstcpwtflocal --account-key $localStorageKey --container-name library --include c --num-results '*' `
            --query "{pending: length([?properties.copy.status=='pending']), failed: length([?properties.copy.status=='failed' || properties.copy.status=='aborted'])}" --output json `
            | ConvertFrom-Json;
        if ($copies.pending -gt 0) {
            Write-Host "  $($copies.pending) of $blobCount still copying";
            Start-Sleep -Seconds 7;
        }
    } while ($copies.pending -gt 0);
    if ($copies.failed -gt 0) {
        Write-Yellow "$($copies.failed) library blobs failed to copy; their covers will be broken images locally.";
    }
}
else {
    Write-Cyan 'Not copying sql database or library covers from prod to localdev.';
}
Write-Host;

Write-Cyan 'Running dotnet tool restore....';
dotnet tool restore;
Write-Host;

Write-Host -ForegroundColor Green 'Done!';
