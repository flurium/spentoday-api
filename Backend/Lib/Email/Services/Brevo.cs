namespace Backend.Lib.Email.Services;

/// <summary>
/// Brevo email sender. At the current moment Brevo doesn't have month limit.
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
        try
        {
            string jsonBody = $@"
            {{
                ""sender"": {{ ""name"": ""{fromName}"", ""email"": ""{fromEmail}"" }},
                ""to"":[{string.Join(",", to.Select(x => $@"{{ ""email"": ""{x}"" }}"))}],
                ""subject"": ""{subject}"",
                ""htmlContent"": ""{html}"",
                ""textContent"": ""{text}""
            }}".Compact();

            HttpResponseMessage response = await Http.JsonPost(httpClient, url, jsonBody);

            //string responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseBody);

            if (response.IsSuccessStatusCode) return EmailStatus.Success;
            return EmailStatus.Failed;
        }
        catch
        {
            return EmailStatus.Failed;
        }
    }
}