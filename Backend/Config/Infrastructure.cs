using Backend.Services;
using Lib;
using Lib.Email;
using Lib.Email.Services;
using Lib.Storage;
using Lib.Storage.Services;

namespace Backend.Config;

public static class Infrastructure
{
    public static void AddEmail(this IServiceCollection services)
    {
        services.AddSingleton<IEmailSender>(_ => new EmailGod(
            new EmailService(new Resend(Secrets.RESEND_API_KEY), new DayLimiter(100)),
            new EmailService(new Brevo(Secrets.BREVO_API_KEY), new DayLimiter(300)),
            new EmailService(new SendGrid(Secrets.SENDGRID_API_KEY), new DayLimiter(100))
        ));
    }

    public static void AddStorage(this IServiceCollection services)
    {
        services.AddScoped<IStorage>(_ => new Storj(
            Secrets.STORJ_ACCESS_KEY, Secrets.STORJ_SECRET_KEY,
            Secrets.STORJ_ENDPOINT, Secrets.STORJ_PUBLIC_KEY, "shops"
        ));
    }

    public static void AddDomainService(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped(provider =>
        {
            var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
            return new DomainService(
                client, Secrets.VERCEL_TOKEN,
                Secrets.VERCEL_PROJECT_ID, Secrets.VERCEL_TEAM_ID
            );
        });
    }
}