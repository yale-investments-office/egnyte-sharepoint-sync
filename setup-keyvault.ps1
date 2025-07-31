# Azure Key Vault Setup Script
# This script creates a Key Vault and stores the Egnyte and SharePoint secrets

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "East US",
    
    [Parameter(Mandatory=$true)]
    [string]$EgnyteClientId,
    
    [Parameter(Mandatory=$true)]
    [string]$EgnyteClientSecret,
    
    [Parameter(Mandatory=$true)]
    [string]$AzureClientId,
    
    [Parameter(Mandatory=$true)]
    [string]$AzureClientSecret,
    
    [Parameter(Mandatory=$true)]
    [string]$AzureTenantId
)

Write-Host "Setting up Azure Key Vault for Egnyte SharePoint Sync" -ForegroundColor Green

# Create Resource Group if it doesn't exist
Write-Host "Creating resource group: $ResourceGroupName" -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# Create Key Vault
Write-Host "Creating Key Vault: $KeyVaultName" -ForegroundColor Yellow
az keyvault create --name $KeyVaultName --resource-group $ResourceGroupName --location $Location

# Set secrets
Write-Host "Storing Egnyte secrets..." -ForegroundColor Yellow
az keyvault secret set --vault-name $KeyVaultName --name "egnyte-client-id" --value $EgnyteClientId
az keyvault secret set --vault-name $KeyVaultName --name "egnyte-client-secret" --value $EgnyteClientSecret

Write-Host "Storing Azure/SharePoint secrets..." -ForegroundColor Yellow
az keyvault secret set --vault-name $KeyVaultName --name "azure-client-id" --value $AzureClientId
az keyvault secret set --vault-name $KeyVaultName --name "azure-client-secret" --value $AzureClientSecret
az keyvault secret set --vault-name $KeyVaultName --name "azure-tenant-id" --value $AzureTenantId

# Get the Key Vault URL
$keyVaultUrl = "https://$KeyVaultName.vault.azure.net/"
Write-Host "Key Vault URL: $keyVaultUrl" -ForegroundColor Green

# Output environment variable settings
Write-Host "`n=== Update your environment variables ===" -ForegroundColor Cyan
Write-Host "KEY_VAULT_URL=$keyVaultUrl" -ForegroundColor White
Write-Host "`nSecrets stored in Key Vault:" -ForegroundColor Cyan
Write-Host "- egnyte-client-id" -ForegroundColor White
Write-Host "- egnyte-client-secret" -ForegroundColor White
Write-Host "- azure-client-id" -ForegroundColor White
Write-Host "- azure-client-secret" -ForegroundColor White
Write-Host "- azure-tenant-id" -ForegroundColor White

Write-Host "`nKey Vault setup complete!" -ForegroundColor Green
