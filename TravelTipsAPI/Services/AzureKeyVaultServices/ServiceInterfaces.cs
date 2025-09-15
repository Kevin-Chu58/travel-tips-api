using Azure.Security.KeyVault.Secrets;

namespace TravelTipsAPI.Services.AzureKeyVaultServices
{
    public class AzureKeyVaultSchema
    {
        public interface IKeyVaultService
        {
            Task<string> GetJsonSecretAsync(string secretName);
        }
    }
}
