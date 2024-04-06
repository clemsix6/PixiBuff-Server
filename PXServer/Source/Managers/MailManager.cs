using System.Net;
using System.Net.Mail;
using PXServer.Source.Database;
using PXServer.Source.Exceptions;


namespace PXServer.Source.Managers;


public class MailManager : Manager
{
    private readonly SmtpClient smtpClient;


    public MailManager(MongoDbContext database) : base(database)
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST") ??
                   throw new ServerException(
                       "SMTP_HOST environment variable is not set", StatusCodes.Status500InternalServerError
                   );
        var port = int.Parse(
            Environment.GetEnvironmentVariable("SMTP_PORT") ??
            throw new ServerException(
                "SMTP_PORT environment variable is not set", StatusCodes.Status500InternalServerError
            )
        );
        var user = Environment.GetEnvironmentVariable("EMAIL_USER") ??
                   throw new ServerException(
                       "EMAIL_USER environment variable is not set", StatusCodes.Status500InternalServerError
                   );
        var password = Environment.GetEnvironmentVariable("EMAIL_PASS") ??
                       throw new ServerException(
                           "EMAIL_PASS environment variable is not set", StatusCodes.Status500InternalServerError
                       );

        this.smtpClient = new SmtpClient(host, port);
        this.smtpClient.Credentials = new NetworkCredential(user, password);
        this.smtpClient.EnableSsl = true;
    }


    public void SendMail(string to, string subject, string body)
    {
        var from = Environment.GetEnvironmentVariable("EMAIL_USER") ??
                   throw new ServerException(
                       "EMAIL_USER environment variable is not set", StatusCodes.Status500InternalServerError
                   );

        var mail = new MailMessage(from, to, subject, body);
        this.smtpClient.Send(mail);
    }
}