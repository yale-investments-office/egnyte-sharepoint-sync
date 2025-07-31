using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EgnyteSharePointSync
{
    public class ConfigurationService
    {
        private readonly KeyVaultService _keyVaultService;
        private readonly ILogger _logger;
        private readonly bool _useKeyVault;

        public ConfigurationService(ILogger logger)
        {
            _logger = logger;
            
            var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
            _useKeyVault = !string.IsNullOrEmpty(keyVaultUrl);

            if (_useKeyVault)
            {
                _keyVaultService = new KeyVaultService(keyVaultUrl, logger);
                _logger.LogInformation("Using Azure Key Vault for configuration");
            }
            else
            {
                _logger.LogInformation("Using environment variables for configuration");
            }
        }

        public async Task<string> GetEgnyteClientIdAsync()
        {
            if (_useKeyVault)
            {
                var (clientId, _) = await _keyVaultService.GetEgnyteCredentialsAsync();
                return clientId;
            }
            
            return Environment.GetEnvironmentVariable("EGNYTE_CLIENT_ID") 
                ?? throw new InvalidOperationException("EGNYTE_CLIENT_ID not configured");
        }

        public async Task<string> GetEgnyteClientSecretAsync()
        {
            if (_useKeyVault)
            {
                var (_, clientSecret) = await _keyVaultService.GetEgnyteCredentialsAsync();
                return clientSecret;
            }
            
            return Environment.GetEnvironmentVariable("EGNYTE_CLIENT_SECRET") 
                ?? throw new InvalidOperationException("EGNYTE_CLIENT_SECRET not configured");
        }

        public string GetEgnyteDomain()
        {
            return Environment.GetEnvironmentVariable("EGNYTE_DOMAIN") 
                ?? throw new InvalidOperationException("EGNYTE_DOMAIN not configured");
        }

        public string GetEgnyteRedirectUri()
        {
            return Environment.GetEnvironmentVariable("EGNYTE_REDIRECT_URI") 
                ?? "http://localhost:3000";
        }

        public async Task<string> GetAzureClientIdAsync()
        {
            if (_useKeyVault)
            {
                var (clientId, _, _) = await _keyVaultService.GetSharePointCredentialsAsync();
                return clientId;
            }
            
            return Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") 
                ?? throw new InvalidOperationException("AZURE_CLIENT_ID not configured");
        }

        public async Task<string> GetAzureClientSecretAsync()
        {
            if (_useKeyVault)
            {
                var (_, clientSecret, _) = await _keyVaultService.GetSharePointCredentialsAsync();
                return clientSecret;
            }
            
            return Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") 
                ?? throw new InvalidOperationException("AZURE_CLIENT_SECRET not configured");
        }

        public async Task<string> GetAzureTenantIdAsync()
        {
            if (_useKeyVault)
            {
                var (_, _, tenantId) = await _keyVaultService.GetSharePointCredentialsAsync();
                return tenantId;
            }
            
            return Environment.GetEnvironmentVariable("AZURE_TENANT_ID") 
                ?? throw new InvalidOperationException("AZURE_TENANT_ID not configured");
        }

        public string GetAzureRedirectUri()
        {
            return Environment.GetEnvironmentVariable("AZURE_REDIRECT_URI") 
                ?? "http://localhost:3000";
        }
    }
}
