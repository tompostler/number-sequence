Write-Host -ForegroundColor Cyan 'Logging in to subscription....';
$subscriptionId = '18fac277-c9ad-4c2d-8172-bd41083dae75';
$context = Get-AzContext;
if ($null -eq $context) {
    Login-AzAccount -SubscriptionId $subscriptionId;
}
elseif ($context.Subscription.Id -ne $subscriptionId) {
    Select-AzSubscription -SubscriptionId $subscription -ErrorAction Stop;
}
Write-Host;

# Note: the following needs to be kept up-to-date with any necessary config changes
Write-Host -ForegroundColor Cyan 'Generating local settings....';
$localSettings = [PSCustomObject]@{
    CosmosDB = [PSCustomObject]@{
        Endpoint    = 'https://tompostler-free.documents.azure.com:443/';
        Key         = (Get-AzCosmosDBAccountKey -ResourceGroupName tompostler -Name tompostler-free).SecondaryMasterKey;
        DatabaseId  = 'shared';
        ContainerId = 'nstcpwtflocal';
    };
    Email    = [PSCustomObject]@{
        Server   = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-server -AsPlainText);
        Port     = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-port -AsPlainText);
        Username = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-username -AsPlainText);
        Password = (Get-AzKeyVaultSecret -VaultName tompostler -Name email-password -AsPlainText);
    };
    Google   = [PSCustomObject]@{
        Credentials = (Get-AzKeyVaultSecret -VaultName tompostler -Name google-dr-chiro-credentials -AsPlainText).Replace('\\"', '"');
    };
    Sql      = [PSCustomObject]@{
        ConnectionString = (
            'Server=tcp:tompostler.database.windows.net,1433;Initial Catalog=nslocal;Persist Security Info=False;' `
                + 'User ID=sqladmin;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Password=' `
                + (Get-AzKeyVaultSecret -VaultName tompostler -Name tompostler-sqladmin-password -AsPlainText) `
                + ';');
    };
    Storage  = [PSCustomObject]@{
        ConnectionString = (
            'DefaultEndpointsProtocol=https;AccountName=nstcpwtf;AccountKey=' `
                + ((Get-AzStorageAccountKey -ResourceGroupName tcp-wtf-hosting -Name nstcpwtf)[1].Value) `
                + ';EndpointSuffix=core.windows.net');
    };
};
$localSettingsPath = Join-Path ($PSScriptRoot) '.\src\number-sequence\appsettings.Development.json';
# Create the item (including path!) if it doesn't exist
New-Item -Path $localSettingsPath -ItemType File -Force | Out-Null;
$localSettings | ConvertTo-Json | Set-Content -Path $localSettingsPath;
Write-Host

Write-Host -ForegroundColor Green 'Done!'
