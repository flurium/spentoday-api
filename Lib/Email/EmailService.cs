namespace Lib.Email;

public record EmailService(IEmailSender Sender, params ILimiter[] Limiters);
