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

    Write-Cyan 'Creating copy of production sql database to nslocal....';
    az sql db copy --resource-group tompostler --server tompostler --name ns --dest-resource-group tompostler --dest-server tompostler --dest-name nslocal --output none;
    Write-Host;

    Write-Cyan 'Resizing nslocal sql database to Basic....';
    az sql db update --resource-group tompostler --server tompostler --name nslocal --service-objective Basic --output none;
    Write-Host;

    # Library covers are flagged in sql (LibraryItem.HasCover) but stored in blob, so they're mirrored along with the
    # database, or every copied cover is a broken image locally. Every blob is overwritten, never skipped: a replaced cover
    # keeps its name, and a local-only item can share an id with a later prod one. Local-only blobs are left; nothing shows
    # a cover its item isn't flagged for. The pdf container isn't copied: those are patient documents, and nothing local
    # needs them.
    Write-Cyan 'Copying library container from nstcpwtf to nstcpwtflocal....';
    $prodStorageKey = az storage account keys list --resource-group tcp-wtf-hosting --account-name nstcpwtf --query '[1].value' --output tsv;
    $localStorageKey = az storage account keys list --resource-group tcp-wtf-hosting --account-name nstcpwtflocal --query '[1].value' --output tsv;
    az storage blob copy start-batch --account-name nstcpwtflocal --account-key $localStorageKey --destination-container library `
        --source-account-name nstcpwtf --source-account-key $prodStorageKey --source-container library --output none;
}
else {
    Write-Cyan 'Not copying sql database or library covers from prod to localdev.';
}
Write-Host;

Write-Cyan 'Running dotnet tool restore....';
dotnet tool restore;
Write-Host;

Write-Host -ForegroundColor Green 'Done!';
