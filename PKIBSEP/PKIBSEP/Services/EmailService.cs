using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PKIBSEP.Interfaces;

namespace PKIBSEP.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string confirmationLink)
        {
            var subject = "Aktivacija naloga - PKI BSEP";
            var body = $@"
                <html>
                <body>
                    <h2>Dobrodošli u PKI BSEP sistem!</h2>
                    <p>Hvala što ste se registrovali. Molimo vas da aktivirate svoj nalog klikom na link ispod:</p>
                    <p><a href='{confirmationLink}'>Aktiviraj nalog</a></p>
                    <p>Ili kopirajte sledeći link u vaš pretraživač:</p>
                    <p>{confirmationLink}</p>
                    <p><strong>Napomena:</strong> Ovaj link je validan 24 sata i može se iskoristiti samo jednom.</p>
                    <br/>
                    <p>Ukoliko niste vi kreirali nalog, molimo vas da ignorišete ovaj email.</p>
                    <br/>
                    <p>Srdačan pozdrav,<br/>PKI BSEP Tim</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetAsync(string toEmail, string resetLink)
        {
            var subject = "Resetovanje lozinke - PKI BSEP";
            var body = $@"
                <html>
                <body>
                    <h2>Zahtev za resetovanje lozinke</h2>
                    <p>Primili smo zahtev za resetovanje vaše lozinke. Kliknite na link ispod da nastavite:</p>
                    <p><a href='{resetLink}'>Resetuj lozinku</a></p>
                    <p>Ili kopirajte sledeći link u vaš pretraživač:</p>
                    <p>{resetLink}</p>
                    <p><strong>Napomena:</strong> Ovaj link je validan 1 sat i može se iskoristiti samo jednom.</p>
                    <br/>
                    <p>Ukoliko niste vi zatražili resetovanje lozinke, molimo vas da ignorišete ovaj email i vaš nalog će ostati siguran.</p>
                    <br/>
                    <p>Srdačan pozdrav,<br/>PKI BSEP Tim</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(username, password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Email uspešno poslat na adresu: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška prilikom slanja emaila na adresu: {Email}", toEmail);
                throw;
            }
        }
    }
}
