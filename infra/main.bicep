targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that will be used to name and group all the resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources.')
param location string

@description('Name of the resource group.')
param resourceGroupName string = 'rg-${environmentName}'

@description('Egnyte domain for the application')
param egnyteDomain string = 'yaleinvestments.egnyte.com'

@description('Frontend URL for CORS and redirects')
param frontendUrl string = ''

@description('Egnyte redirect URI')
param egnyteRedirectUri string = ''

@description('Azure redirect URI')
param azureRedirectUri string = ''

// Generate a unique token for resource naming
var resourceToken = uniqueString(subscription().id, location, environmentName)
var resourcePrefix = 'egs'

// Create the resource group
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Deploy resources within the resource group
module resources 'main-resources.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    environmentName: environmentName
    location: location
    resourceToken: resourceToken
    resourcePrefix: resourcePrefix
    egnyteDomain: egnyteDomain
    frontendUrl: frontendUrl
    egnyteRedirectUri: egnyteRedirectUri
    azureRedirectUri: azureRedirectUri
  }
}

// Outputs
output RESOURCE_GROUP_ID string = rg.id
output AZURE_LOCATION string = location
output AZURE_KEY_VAULT_NAME string = resources.outputs.AZURE_KEY_VAULT_NAME
output AZURE_KEY_VAULT_URL string = resources.outputs.AZURE_KEY_VAULT_URL
output AZURE_FUNCTION_APP_NAME string = resources.outputs.AZURE_FUNCTION_APP_NAME
output AZURE_STATIC_WEB_APP_NAME string = resources.outputs.AZURE_STATIC_WEB_APP_NAME
output BACKEND_URL string = resources.outputs.BACKEND_URL
output FRONTEND_URL string = resources.outputs.FRONTEND_URL
