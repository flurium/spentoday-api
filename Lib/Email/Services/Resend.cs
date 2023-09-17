using System.Net;

namespace Lib.Email.Services;

/// <summary>
/// Send emails with Resend service: <see href="https://resend.com/">resend.com</see>
/// </summary>
public class Resend : IEmailSender
{
    private const string url = "https://api.resend.com/emails";

    private readonly HttpClient httpClient;

    public Resend(string token)
    {
        httpClient = Http.JsonClient(headers =>
        {
            headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        });
    }

    public async Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html)
    {
        string jsonBody = $@"
        {{
            ""from"":""{fromName} <{fromEmail}>"",
            ""to"":[{string.Join(",", toEmails.Select(x => $@"""{x}"""))}],
            ""subject"":""{subject}"",
            ""text"":""{text}"",
            ""html"":""{html}""
        }}".Compact();

        var response = await Http.JsonPost(httpClient, url, jsonBody);

        if (response == null) return EmailStatus.Failed;

        if (response.IsSuccessStatusCode) return EmailStatus.Success;

        if (response.StatusCode == HttpStatusCode.TooManyRequests) return EmailStatus.LimitReached;

        return EmailStatus.Failed;
    }
}