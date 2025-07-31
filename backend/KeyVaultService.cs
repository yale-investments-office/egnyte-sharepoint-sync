using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EgnyteSharePointSync
{
    public class KeyVaultService
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger _logger;

        public KeyVaultService(string keyVaultUrl, ILogger logger)
        {
            _logger = logger;
            
            // Use DefaultAzureCredential which supports multiple authentication methods:
            // - Environment variables (for local development)
            // - Managed Identity (for Azure deployment)
            // - Azure CLI (for local development)
            var credential = new DefaultAzureCredential();
            _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            try
            {
                _logger.LogInformation($"Retrieving secret: {secretName}");
                var response = await _secretClient.GetSecretAsync(secretName);
                return response.Value.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve secret: {secretName}");
                throw;
            }
        }

        public async Task<(string clientId, string clientSecret)> GetEgnyteCredentialsAsync()
        {
            try
            {
                var clientIdTask = GetSecretAsync("egnyte-client-id");
                var clientSecretTask = GetSecretAsync("egnyte-client-secret");

                await Task.WhenAll(clientIdTask, clientSecretTask);

                return (await clientIdTask, await clientSecretTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve Egnyte credentials from Key Vault");
                throw;
            }
        }

        public async Task<(string clientId, string clientSecret, string tenantId)> GetSharePointCredentialsAsync()
        {
            try
            {
                var clientIdTask = GetSecretAsync("azure-client-id");
                var clientSecretTask = GetSecretAsync("azure-client-secret");
                var tenantIdTask = GetSecretAsync("azure-tenant-id");

                await Task.WhenAll(clientIdTask, clientSecretTask, tenantIdTask);

                return (await clientIdTask, await clientSecretTask, await tenantIdTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve SharePoint credentials from Key Vault");
                throw;
            }
        }
    }
}
