#!/bin/bash

# Azure Key Vault Setup Script
# This script creates a Key Vault and stores the Egnyte and SharePoint secrets

set -e

# Function to display usage
usage() {
    echo "Usage: $0 --resource-group <name> --keyvault-name <name> --location <location> \\"
    echo "          --egnyte-client-id <id> --egnyte-client-secret <secret> \\"
    echo "          --azure-client-id <id> --azure-client-secret <secret> --azure-tenant-id <id>"
    echo ""
    echo "Example:"
    echo "  $0 --resource-group my-rg --keyvault-name my-kv --location 'East US' \\"
    echo "     --egnyte-client-id 'your-egnyte-id' --egnyte-client-secret 'your-egnyte-secret' \\"
    echo "     --azure-client-id 'your-azure-id' --azure-client-secret 'your-azure-secret' \\"
    echo "     --azure-tenant-id 'your-tenant-id'"
    exit 1
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        --keyvault-name)
            KEYVAULT_NAME="$2"
            shift 2
            ;;
        --location)
            LOCATION="$2"
            shift 2
            ;;
        --egnyte-client-id)
            EGNYTE_CLIENT_ID="$2"
            shift 2
            ;;
        --egnyte-client-secret)
            EGNYTE_CLIENT_SECRET="$2"
            shift 2
            ;;
        --azure-client-id)
            AZURE_CLIENT_ID="$2"
            shift 2
            ;;
        --azure-client-secret)
            AZURE_CLIENT_SECRET="$2"
            shift 2
            ;;
        --azure-tenant-id)
            AZURE_TENANT_ID="$2"
            shift 2
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo "Unknown option $1"
            usage
            ;;
    esac
done

# Check required parameters
if [ -z "$RESOURCE_GROUP" ] || [ -z "$KEYVAULT_NAME" ] || [ -z "$EGNYTE_CLIENT_ID" ] || \
   [ -z "$EGNYTE_CLIENT_SECRET" ] || [ -z "$AZURE_CLIENT_ID" ] || \
   [ -z "$AZURE_CLIENT_SECRET" ] || [ -z "$AZURE_TENANT_ID" ]; then
    echo "Error: Missing required parameters"
    usage
fi

# Set default location if not provided
LOCATION="${LOCATION:-East US}"

echo "🔧 Setting up Azure Key Vault for Egnyte SharePoint Sync"
echo "========================================================="

# Create Resource Group if it doesn't exist
echo "📁 Creating resource group: $RESOURCE_GROUP"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output table

# Create Key Vault
echo "🔐 Creating Key Vault: $KEYVAULT_NAME"
az keyvault create --name "$KEYVAULT_NAME" --resource-group "$RESOURCE_GROUP" --location "$LOCATION" --output table

# Set secrets
echo "💾 Storing Egnyte secrets..."
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "egnyte-client-id" --value "$EGNYTE_CLIENT_ID" --output none
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "egnyte-client-secret" --value "$EGNYTE_CLIENT_SECRET" --output none

echo "💾 Storing Azure/SharePoint secrets..."
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "azure-client-id" --value "$AZURE_CLIENT_ID" --output none
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "azure-client-secret" --value "$AZURE_CLIENT_SECRET" --output none
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "azure-tenant-id" --value "$AZURE_TENANT_ID" --output none

# Get the Key Vault URL
KEYVAULT_URL="https://$KEYVAULT_NAME.vault.azure.net/"

echo ""
echo "✅ Key Vault setup complete!"
echo "=========================="
echo "🔗 Key Vault URL: $KEYVAULT_URL"
echo ""
echo "📋 Update your environment variables:"
echo "KEY_VAULT_URL=$KEYVAULT_URL"
echo ""
echo "🔑 Secrets stored in Key Vault:"
echo "- egnyte-client-id"
echo "- egnyte-client-secret"
echo "- azure-client-id"
echo "- azure-client-secret"
echo "- azure-tenant-id"
echo ""
echo "🚀 You can now deploy your application with secure credential management!"
