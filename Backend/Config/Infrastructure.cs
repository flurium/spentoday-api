using Lib;
using Lib.Email.Services;
using Lib.Email;

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
}