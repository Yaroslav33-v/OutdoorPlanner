using System.Net.Mail;
using System.Net;

namespace OutdoorPlanner.Email
{
    internal class EmailConfiguration
    {
        public MailAddress fromMailAddress { get; }
        public SmtpClient smtpClient { get; }
        public EmailConfiguration(string fromMailAddress, string password, string smtpServer, int smtpPort)
        {
            this.fromMailAddress = new MailAddress(fromMailAddress);
            smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(fromMailAddress, password),
                EnableSsl = true
            };
        }
    }
}
