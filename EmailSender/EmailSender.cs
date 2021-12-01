using System;
using System.Net;
using System.Net.Mail;

namespace EmailSender_ns
{
    public class EmailSender
    {
        static string emailAddress = "skejouler@gmail.com";

        static string password = "table7Skejoule";

        SmtpClient smtpClient = new("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(emailAddress, password),
            EnableSsl = true,
        };

        public void SendEmail(string receiver, string subject, string body)
        {
            smtpClient.Send(emailAddress, receiver, subject, body);
        }

        public static bool IsValidEmail(string email)
        {
            if (email.Trim().EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
