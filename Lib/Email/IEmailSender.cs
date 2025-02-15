namespace Lib.Email;

public interface IEmailSender
{
    public Task<EmailStatus> Send(string fromEmail, string fromName, List<string> toEmails, string subject, string text, string html);
}
