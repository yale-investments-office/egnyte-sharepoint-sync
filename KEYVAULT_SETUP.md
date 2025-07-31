# Azure Key Vault Integration Guide

## ✅ Current Status (Updated July 30, 2025)

**Key Vault Setup: COMPLETE** 🎉
- **Key Vault Name**: `egnytespkv-1753929214`
- **Key Vault URL**: `https://egnytespkv-1753929214.vault.azure.net/`
- **Resource Group**: `egnyte-sharepoint-sync-rg`
- **RBAC Permissions**: ✅ Assigned (Key Vault Secrets Officer)

**Secrets Stored:**
- ✅ `azure-client-id`: `90cf08a3-b3a4-4c02-9e9c-d7b93e7b2b6b`
- ✅ `azure-client-secret`: `[STORED SECURELY]`
- ✅ `azure-tenant-id`: `cf198459-871a-4cec-aa12-7bedb6b4f602`
- ⏳ `egnyte-client-id`: `PLACEHOLDER_EGNYTE_CLIENT_ID` (needs real value)
- ⏳ `egnyte-client-secret`: `PLACEHOLDER_EGNYTE_CLIENT_SECRET` (needs real value)

**Application Status:**
- ✅ Backend Functions: Running on `localhost:7071`
- ✅ Frontend React App: Running on `localhost:3002`
- ✅ Mock APIs: Working for UI testing
- ✅ SharePoint Auth: Working with real Azure credentials

**Next Steps:**
1. **Get Egnyte API Credentials** (see [Egnyte Setup](#egnyte-api-setup) below)
2. **Update Key Vault with Real Egnyte Values**
3. **Test End-to-End Sync Workflow**

---

## Prerequisites

- Azure CLI installed and logged in
- Azure subscription with permissions to create resources
- Egnyte API credentials (Client ID and Secret)
- Azure/Microsoft Graph API credentials for SharePoint access

## Quick Setup

### Option 1: Using the Setup Script (Recommended)

```bash
# Make the script executable
chmod +x setup-keyvault.sh

# Run the setup script
./setup-keyvault.sh \
  --resource-group "egnyte-sharepoint-sync-rg" \
  --keyvault-name "egnytespkv-1753929214" \
  --location "eastus" \
  --egnyte-client-id "your-egnyte-client-id" \
  --egnyte-client-secret "your-egnyte-client-secret" \
  --azure-client-id "90cf08a3-b3a4-4c02-9e9c-d7b93e7b2b6b" \
  --azure-client-secret "o5L8Q~hpRBTYFkllhNPDGc3V7y4hKKBsz7V8zcWK" \
  --azure-tenant-id "cf198459-871a-4cec-aa12-7bedb6b4f602"
```

### Option 2: Manual Setup

1. **Create Resource Group**
```bash
az group create --name "egnyte-sharepoint-sync-rg" --location "East US"
```

2. **Create Key Vault**
```bash
az keyvault create \
  --name "egnytespkv-1753929214" \
  --resource-group "egnyte-sharepoint-sync-rg" \
  --location "eastus"
```

3. **Store Secrets**
```bash
# Egnyte credentials
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "egnyte-client-id" --value "your-egnyte-client-id"
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "egnyte-client-secret" --value "your-egnyte-client-secret"

# Azure/SharePoint credentials  
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "azure-client-id" --value "90cf08a3-b3a4-4c02-9e9c-d7b93e7b2b6b"
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "azure-client-secret" --value "o5L8Q~hpRBTYFkllhNPDGc3V7y4hKKBsz7V8zcWK"
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "azure-tenant-id" --value "cf198459-871a-4cec-aa12-7bedb6b4f602"
```

## Configuration

### Environment Variables

After setting up Key Vault, update your environment variables:

**For Production (using Key Vault):**
```bash
KEY_VAULT_URL=https://egnytespkv-1753929214.vault.azure.net/
EGNYTE_DOMAIN=your-egnyte-domain
EGNYTE_REDIRECT_URI=https://your-app-domain.com
AZURE_REDIRECT_URI=https://your-app-domain.com
```

**For Local Development (optional - can still use Key Vault):**
```bash
# Option 1: Use Key Vault (recommended)
KEY_VAULT_URL=https://egnytespkv-1753929214.vault.azure.net/
EGNYTE_DOMAIN=your-egnyte-domain
EGNYTE_REDIRECT_URI=http://localhost:3000
AZURE_REDIRECT_URI=http://localhost:3000

# Option 2: Use local environment variables
EGNYTE_DOMAIN=your-egnyte-domain
EGNYTE_CLIENT_ID=your-egnyte-client-id
EGNYTE_CLIENT_SECRET=your-egnyte-client-secret
EGNYTE_REDIRECT_URI=http://localhost:3000
AZURE_CLIENT_ID=90cf08a3-b3a4-4c02-9e9c-d7b93e7b2b6b
AZURE_CLIENT_SECRET=o5L8Q~hpRBTYFkllhNPDGc3V7y4hKKBsz7V8zcWK
AZURE_TENANT_ID=cf198459-871a-4cec-aa12-7bedb6b4f602
AZURE_REDIRECT_URI=http://localhost:3000
```

## Authentication Methods

The application supports multiple authentication methods for accessing Key Vault:

### 1. Managed Identity (Production - Recommended)
When deployed to Azure, the app uses the system-assigned managed identity automatically.

### 2. Service Principal (CI/CD)
For automated deployments, use a service principal:
```bash
# Set environment variables for service principal
AZURE_CLIENT_ID=your-service-principal-id
AZURE_CLIENT_SECRET=your-service-principal-secret
AZURE_TENANT_ID=your-tenant-id
```

### 3. Azure CLI (Local Development)
For local development, authenticate using Azure CLI:
```bash
az login
```

### 4. Environment Variables (Local Development)
Use the same service principal environment variables as option 2.

## Secret Names in Key Vault

The application expects these exact secret names in Key Vault:

| Secret Name | Description |
|-------------|-------------|
| `egnyte-client-id` | Egnyte API Client ID |
| `egnyte-client-secret` | Egnyte API Client Secret |
| `azure-client-id` | Azure/Microsoft Graph Client ID |
| `azure-client-secret` | Azure/Microsoft Graph Client Secret |
| `azure-tenant-id` | Azure Tenant ID |

## Permissions

### Key Vault Access Policy
The application identity needs the following permissions:
- **Secret permissions**: Get, List

### Azure App Registration
For SharePoint access, your Azure app registration needs:
- **Microsoft Graph API permissions**:
  - Sites.Read.All
  - Files.ReadWrite.All
  - User.Read

## Deployment Configuration

### Azure Functions App Settings
When deploying to Azure Functions, set these application settings:

```bash
az functionapp config appsettings set \
  --name your-function-app-name \
  --resource-group your-resource-group \
  --settings \
    KEY_VAULT_URL="https://your-keyvault-name.vault.azure.net/" \
    EGNYTE_DOMAIN="your-egnyte-domain" \
    EGNYTE_REDIRECT_URI="https://your-app-domain.com" \
    AZURE_REDIRECT_URI="https://your-app-domain.com"
```

### Managed Identity Setup
Enable system-assigned managed identity for your Azure Functions app:

```bash
az functionapp identity assign \
  --name your-function-app-name \
  --resource-group your-resource-group
```

Grant the managed identity access to Key Vault:

```bash
# Get the principal ID of the managed identity
PRINCIPAL_ID=$(az functionapp identity show \
  --name your-function-app-name \
  --resource-group your-resource-group \
  --query principalId --output tsv)

# Grant Key Vault access
az keyvault set-policy \
  --name your-keyvault-name \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

## Testing

### Local Testing with Key Vault
1. Authenticate with Azure CLI: `az login`
2. Set the `KEY_VAULT_URL` environment variable
3. Run your application - it will automatically use Azure CLI credentials

### Local Testing without Key Vault
1. Remove or comment out the `KEY_VAULT_URL` environment variable
2. Set the individual credential environment variables
3. Run your application - it will use the local environment variables

## Troubleshooting

### Common Issues

1. **"Failed to retrieve secret" errors**
   - Check that you're authenticated to Azure
   - Verify Key Vault URL is correct
   - Ensure you have proper permissions

2. **"Key Vault not found" errors**
   - Verify the Key Vault name and URL
   - Check that the Key Vault exists in the correct subscription

3. **Authentication failures**
   - For local development, run `az login`
   - For production, ensure managed identity is enabled and has proper permissions

### Debug Mode
Set the logging level to Debug to see detailed Key Vault operations:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Security Best Practices

1. **Never store secrets in code or configuration files**
2. **Use managed identities when possible**
3. **Regularly rotate secrets**
4. **Monitor Key Vault access logs**
5. **Use least-privilege access policies**
6. **Enable Key Vault firewall and virtual network restrictions**

## Cost Optimization

- Use the Standard tier for production (includes SLA)
- Basic tier is sufficient for development/testing
- Monitor secret operations to optimize costs

---

## Egnyte API Setup

To complete the setup, you need to obtain Egnyte API credentials:

### 1. Access Egnyte Admin Panel
1. Log into your Egnyte domain as an administrator
2. Go to **Settings** → **Integrations** → **API Applications**
3. Click **Create Application** or **Add Application**

### 2. Create OAuth Application
Configure your Egnyte OAuth application with these settings:
- **Application Name**: `Egnyte SharePoint Sync`
- **Application Type**: `Web Application`
- **Redirect URI**: `http://localhost:3002` (for local development)
- **Scopes**: Select the following permissions:
  - `Egnyte.filesystem` (to read and access files)
  - `Egnyte.user` (to get user information)

### 3. Get Credentials
After creating the application, you'll receive:
- **Client ID** (also called Application Key)
- **Client Secret** (also called Shared Secret)

### 4. Update Key Vault
Once you have the real Egnyte credentials, run these commands:
```bash
# Update Egnyte Client ID
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "egnyte-client-id" --value "YOUR_REAL_EGNYTE_CLIENT_ID"

# Update Egnyte Client Secret  
az keyvault secret set --vault-name "egnytespkv-1753929214" --name "egnyte-client-secret" --value "YOUR_REAL_EGNYTE_CLIENT_SECRET"
```

### 5. Update Environment Variables
Also update your local `.env` file:
```bash
EGNYTE_DOMAIN=your-egnyte-domain.egnyte.com
EGNYTE_CLIENT_ID=YOUR_REAL_EGNYTE_CLIENT_ID
EGNYTE_CLIENT_SECRET=YOUR_REAL_EGNYTE_CLIENT_SECRET
```
