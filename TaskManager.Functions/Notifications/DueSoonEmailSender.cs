using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using TaskManager_Functions.Notifications;

namespace TaskManager.Functions.Notifications;

public class SendGridDueSoonEmailSender : IDueSoonEmailSender
{
    private readonly ISendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<SendGridDueSoonEmailSender> _logger;

    public SendGridDueSoonEmailSender(IConfiguration configuration, ILogger<SendGridDueSoonEmailSender> logger)
    {
        var apiKey = configuration["SendGrid:ApiKey"]
            ?? throw new InvalidOperationException("SendGrid:ApiKey is not configured.");
        _fromEmail = configuration["SendGrid:FromEmail"]
            ?? throw new InvalidOperationException("SendGrid:FromEmail is not configured.");
        _fromName = configuration["SendGrid:FromName"] ?? "Task Manager";
        _client = new SendGridClient(apiKey);
        _logger = logger;
    }

    public async Task SendDueSoonEmailAsync(string toEmail, string toName, string taskTitle, string projectName, DateTime dueDateUtc, CancellationToken cancellationToken = default)
    {
        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_fromEmail, _fromName),
            new EmailAddress(toEmail, toName),
            $"Tarefa vencendo em breve: {taskTitle}",
            $"A tarefa '{taskTitle}' do projeto '{projectName}' vence em {dueDateUtc:g} (UTC).",
            htmlContent: null);

        var response = await _client.SendEmailAsync(message, cancellationToken);

        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("SendGrid email to {Email} failed with status {Status}: {Body}", toEmail, response.StatusCode, body);
        }
    }
}
