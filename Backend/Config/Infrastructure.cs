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
        var resendApiKey = Env.Get("RESEND_API_KEY");
        var brevoApiKey = Env.Get("BREVO_API_KEY");
        var sendGridApiKey = Env.Get("SENDGRID_API_KEY");

        services.AddSingleton<IEmailSender>(_ => new EmailGod(
            new EmailService(new Resend(resendApiKey), new DayLimiter(100)),
            new EmailService(new Brevo(brevoApiKey), new DayLimiter(300)),
            new EmailService(new SendGrid(sendGridApiKey), new DayLimiter(100))
        ));
    }

    public static void AddStorage(this IServiceCollection services)
    {
        var storjAccessKey = Env.Get("STORJ_ACCESS_KEY");
        var storjSecretKey = Env.Get("STORJ_SECRET_KEY");
        var storjEndpoint = Env.Get("STORJ_ENDPOINT");
        var storjPublicKey = Env.Get("STORJ_PUBLIC_KEY");

        services.AddScoped<IStorage>(_ => new Storj(storjAccessKey, storjSecretKey, storjEndpoint, storjPublicKey, "shops"));
    }

    public static void AddDomainService(this IServiceCollection services)
    {
        var token = Env.Get("VERCEL_TOKEN");
        var projectId = Env.Get("VERCEL_PROJECT_ID");
        var teamId = Env.Get("VERCEL_TEAM_ID");

        services.AddHttpClient();
        services.AddScoped(provider =>
        {
            var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
            return new DomainService(client, token, projectId, teamId);
        });
    }
}