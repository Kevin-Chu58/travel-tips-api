using static TravelTipsAPI.Services.AzureKeyVaultServices.AzureKeyVaultSchema;

namespace TravelTipsAPI.Services.AzureKeyVaultServices
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddAzureKeyVaultServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<IKeyVaultService, KeyVaultService>();

            return services;
        }
    }
}
