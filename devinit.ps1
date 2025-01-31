function Write-Cyan([string]$statement) {
    Write-Host -ForegroundColor Cyan "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss.ff'))] $statement";
}
function Write-Yellow([string]$statement) {
    Write-Host -ForegroundColor Yellow "[$((Get-Date).ToString('yyyy-MM-dd HH:mm:ss.ff'))] $statement";
}

Write-Cyan 'Logging in to subscription....';
$subscriptionId = '78560c44-50bb-4840-9d59-84578a99032e';
$context = Get-AzContext;
if ($null -eq $context) {
    Login-AzAccount -SubscriptionId $subscriptionId;
}
elseif ($context.Subscription.Id -ne $subscriptionId) {
    Select-AzSubscription -SubscriptionId $subscription -ErrorAction Stop;
}
Write-Host;

# Note: the following needs to be kept up-to-date with any necessary config changes
Write-Cyan 'Generating local settings....';
$localSettings = [PSCustomObject]@{
    Email   = [PSCustomObject]@{
        ChiroBatchMap      = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-chiro-batch-map -AsPlainText).Replace('\"', '"');
        ChiroBatchUri      = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-chiro-batch-uri -AsPlainText);
        Server             = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-server -AsPlainText);
        Port               = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-port -AsPlainText);
        Username           = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-username -AsPlainText);
        Password           = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-password -AsPlainText);
        LocalDevToOverride = (git config --get user.email);
    };
    Google  = [PSCustomObject]@{
        Credentials = (Get-AzKeyVaultSecret -VaultName tompostler -Name google-dr-chiro-credentials -AsPlainText).Replace('\"', '"');
    };
    Sql     = [PSCustomObject]@{
        ConnectionString = (
            'Server=tcp:tompostler.database.windows.net,1433;Initial Catalog=nslocal;Persist Security Info=False;' `
                + 'User ID=sqladmin;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Password=' `
                + (Get-AzKeyVaultSecret -VaultName tompostler -Name tompostler-sqladmin-password -AsPlainText) `
                + ';');
    };
    Storage = [PSCustomObject]@{
        ConnectionString = (
            'DefaultEndpointsProtocol=https;AccountName=nstcpwtflocal;AccountKey=' `
                + ((Get-AzStorageAccountKey -ResourceGroupName tcp-wtf-hosting -Name nstcpwtflocal)[1].Value) `
                + ';EndpointSuffix=core.windows.net');
    };
};
$localSettingsPath = Join-Path ($PSScriptRoot) '.\src\number-sequence\appsettings.Development.json';
# Create the item (including path!) if it doesn't exist
New-Item -Path $localSettingsPath -ItemType File -Force | Out-Null;
$localSettings | ConvertTo-Json | Set-Content -Path $localSettingsPath;
Write-Host;

$confirm = Read-Host 'Do you wish to also replace the nslocal sql database from prod? Note, this may take several minutes. [yN]';
if ($confirm -eq 'y') {
    Write-Cyan 'Copying sql database from prod to localdev.';
    Write-Host;

    if (Get-AzSqlDatabase -ResourceGroupName tompostler -ServerName tompostler -DatabaseName nslocal -ErrorAction SilentlyContinue) {
        Write-Cyan 'Deleting existing nslocal sql database....';
        Remove-AzSqlDatabase -ResourceGroupName tompostler -ServerName tompostler -DatabaseName nslocal;
    }
    Write-Host;

    Write-Cyan 'Creating copy of production sql database to nslocal....';
    New-AzSqlDatabaseCopy -ResourceGroupName tompostler -ServerName tompostler -DatabaseName ns -CopyResourceGroupName tompostler -CopyServerName tompostler -CopyDatabaseName nslocal;
    Write-Host;

    Write-Cyan 'Resizing nslocal sql database to GP_S_Gen5_1....';
    Set-AzSqlDatabase -ResourceGroupName tompostler -ServerName tompostler -DatabaseName nslocal -RequestedServiceObjectiveName GP_S_Gen5_1;

    Write-Yellow 'You will need to perform manual cleaning of the database for experimentation!';
}
else {
    Write-Cyan 'Not copying sql database from prod to localdev.';
}
Write-Host;

Write-Host -ForegroundColor Green 'Done!';
