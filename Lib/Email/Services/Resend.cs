using Lib.Email;
using System.Net;

namespace Lib.Email.Services;

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
        try
        {
            string jsonBody = $@"
            {{
                ""from"": ""{fromName} <{fromEmail}>"",
                ""to"": [{string.Join(",", toEmails.Select(x => $@"""{x}"""))}],
                ""subject"": ""{subject}"",
                ""text"":""{text}"",
                ""html"": ""{html}""
            }}".Compact();

            HttpResponseMessage response = await Http.JsonPost(httpClient, url, jsonBody);

            //string responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseBody);

            if (response.IsSuccessStatusCode) return EmailStatus.Success;

            if (response.StatusCode == HttpStatusCode.TooManyRequests) return EmailStatus.LimitReached;

            return EmailStatus.Failed;
        }
        catch
        {
            return EmailStatus.Failed;
        }
    }
}