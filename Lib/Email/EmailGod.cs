using System.Collections.Immutable;

namespace Lib.Email;

public enum EmailStatus
{
    Success,
    LimitReached,
    Failed
}

public interface IEmailSender
{
    public Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html);
}

public record EmailService(IEmailSender Sender, params ILimiter[] Limiters);

public class EmailGod : IEmailSender
{
    private readonly IImmutableList<EmailService> services;

    public EmailGod(EmailService service, params EmailService[] services)
    {
        var list = new List<EmailService>(services.Length + 1) { service };
        list.AddRange(services);
        this.services = list.ToImmutableList();
    }

    public async Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html)
    {
        try
        {
            bool allLimitsReached = true;

            foreach (var service in services)
            {
                try
                {
                    if (!service.Limiters.All(l => l.IsLimitAllow())) continue;

                    var status = await service.Sender.Send(fromEmail, fromName, toEmails, subject, text, html);

                    if (status == EmailStatus.Success)
                    {
                        foreach (var limiter in service.Limiters) limiter.IncrementLimiter();
                        return EmailStatus.Success;
                    }

                    if (status == EmailStatus.LimitReached)
                    {
                        foreach (var limiter in service.Limiters) limiter.ReachedLimit();
                    }
                    else
                    {
                        // if (status == EmailStatus.Failed)
                        allLimitsReached = false;
                    }
                }
                catch
                {
                    allLimitsReached = false;
                    continue;
                }
            }

            return allLimitsReached ? EmailStatus.LimitReached : EmailStatus.Failed;
        }
        catch
        {
            return EmailStatus.Failed;
        }
    }
}