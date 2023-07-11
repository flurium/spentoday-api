using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text;

namespace Backend.Lib.Email;

public enum EmailStatus
{
    Success,
    LimitReached,
    Failed
}

public interface IEmailSender
{
    public Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html);

    protected static HttpClient JsonHttpClient(Action<HttpRequestHeaders> setHeaders)
    {
        HttpClient client = new();

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        setHeaders(client.DefaultRequestHeaders);

        return client;
    }

    protected static async Task<HttpResponseMessage> JsonPost(HttpClient client, string url, string body)
    {
        return await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
    }
}

public class EmailService
{
    public IEmailSender Sender { get; }
    public ILimiter[] Limiters { get; }

    public EmailService(IEmailSender sender, params ILimiter[] limiters)
    {
        Sender = sender;
        Limiters = limiters;
    }
}

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
                if (!service.Limiters.All(l => l.IsLimitAllow())) continue;

                var status = await service.Sender.Send(fromEmail, fromName, toEmails, subject, text, html);

                if (status == EmailStatus.Success)
                {
                    foreach (var limiter in service.Limiters) limiter.IncrementLimiter();
                    return EmailStatus.Success;
                }

                if (status == EmailStatus.Failed) allLimitsReached = false;
            }

            return allLimitsReached ? EmailStatus.LimitReached : EmailStatus.Failed;
        }
        catch
        {
            return EmailStatus.Failed;
        }
    }
}