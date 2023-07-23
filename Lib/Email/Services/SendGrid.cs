using System.Net;
using System.Net.Http.Headers;

namespace Lib.Email.Services;

/// <summary>
/// Send emails with SendGrid service: <see href="https://sendgrid.com/">sendgrid.com</see>
/// </summary>
public class SendGrid : IEmailSender
{
    private const string url = "https://api.sendgrid.com/v3/mail/send";
    private readonly HttpClient httpClient;

    public SendGrid(string key)
    {
        httpClient = Http.JsonClient(headers =>
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        });
    }

    public async Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html)
    {
        string jsonBody = @$"{{
            ""personalizations"":[{{
                ""to"":[{string.Join(",", toEmails.Select(x => $@"{{""email"":""{x}""}}"))}]
            }}],
            ""from"":{{
                ""email"":""{fromEmail}"",
                ""name"":""{fromName}""
            }},
            ""reply_to"":{{
                ""email"":""{fromEmail}"",
                ""name"":""{fromName}""
            }},
            ""subject"":""{subject}"",
            ""content"": [
                {{
                    ""type"": ""text/plain"",
                    ""value"": ""{text}""
                }},
                {{
                    ""type"": ""text/html"",
                    ""value"": ""{html}""
                }}
            ]
        }}".Compact();

        var response = await Http.JsonPost(httpClient, url, jsonBody);
        if (response == null) return EmailStatus.Failed;

        if (response.IsSuccessStatusCode) return EmailStatus.Success;
        if (response.StatusCode == HttpStatusCode.TooManyRequests) return EmailStatus.LimitReached;
        return EmailStatus.Failed;
    }
}