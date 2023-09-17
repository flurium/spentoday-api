namespace Lib.Email.Services;

/// <summary>
/// Send emails with Brevo service: <see href="https://www.brevo.com">brevo.com</see>
/// </summary>
public class Brevo : IEmailSender
{
    private const string url = "https://api.brevo.com/v3/smtp/email";
    private readonly HttpClient httpClient;

    public Brevo(string token)
    {
        httpClient = Http.JsonClient(headers => headers.Add("api-key", token));
    }

    public async Task<EmailStatus> Send(string fromEmail, string fromName, List<string> to, string subject, string text, string html)
    {
        string jsonBody = $@"
        {{
            ""sender"":{{""name"":""{fromName}"",""email"":""{fromEmail}""}},
            ""to"":[{string.Join(",", to.Select(x => $@"{{""email"":""{x}""}}"))}],
            ""subject"":""{subject}"",
            ""htmlContent"":""{html}"",
            ""textContent"":""{text}""
        }}".Compact();

        var response = await Http.JsonPost(httpClient, url, jsonBody);
        if (response == null) return EmailStatus.Failed;

        if (response.IsSuccessStatusCode) return EmailStatus.Success;
        return EmailStatus.Failed;
    }
}