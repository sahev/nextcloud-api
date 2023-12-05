namespace NextCloud.Api.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration) =>
            services.Configure <Settings>(configuration.GetSection("NextCloud"));
    }
}
