using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;

namespace TravelTipsAPI.Services.AzureKeyVaultServices
{
    public class KeyVaultService(string keyVaultUrl, ClientSecretCredential credential)
        : IKeyVaultService
    {
        // lock
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly SecretClient secretClient = new(new Uri(keyVaultUrl), credential);
        private string? _secret;

        public async Task<string> GetJsonSecretAsync(string secretName)
        {
            if (_secret is not null)
                return _secret;

            await _lock.WaitAsync();
            try
            {
                if (_secret is null) // double-check
                {
                    KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
                    _secret = secret.Value;
                }
            }
            finally
            {
                _lock.Release();
            }

            return _secret;
        }
    }
}
