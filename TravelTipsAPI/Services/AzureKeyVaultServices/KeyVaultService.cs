using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;

namespace TravelTipsAPI.Services.AzureKeyVaultServices
{
    public class KeyVaultService(string keyVaultUrl, ClientSecretCredential credential)
        : IKeyVaultService
    {
        private readonly SecretClient secretClient = new(new Uri(keyVaultUrl), credential);

        public async Task<string> GetJsonSecretAsync(string secretName)
        {
            KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}
