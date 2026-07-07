using FaithFlow.Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace FaithFlow.Backend.Services;

/// <summary>
/// Email service that logs emails in development.
/// To enable real sending, configure Email:FromAddress and Email:Provider = "SES" in appsettings.
/// Then inject IAmazonSimpleEmailServiceV2 and add AWS SES SDK package.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendAsync(string toAddress, string subject, string htmlBody)
    {
        var provider = _configuration["Email:Provider"] ?? "Log";

        if (string.IsNullOrWhiteSpace(toAddress))
        {
            return Task.CompletedTask;
        }

        // In development / when SES is not configured, just log the email content.
        _logger.LogInformation(
            "[EmailService] Would send email to {To} | Subject: {Subject}",
            toAddress, subject);

        if (!provider.Equals("SES", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        // TODO: Add AWSSDK.SimpleEmailV2 package and replace this stub with real SES v2 sending:
        //
        // var sesClient = serviceProvider.GetRequiredService<IAmazonSimpleEmailServiceV2>();
        // await sesClient.SendEmailAsync(new SendEmailRequest {
        //     FromEmailAddress = _configuration["Email:FromAddress"],
        //     Destination = new Destination { ToAddresses = new List<string> { toAddress } },
        //     Content = new EmailContent {
        //         Simple = new Message {
        //             Subject = new Content { Data = subject },
        //             Body = new Body { Html = new Content { Data = htmlBody } }
        //         }
        //     }
        // });

        return Task.CompletedTask;
    }
}
