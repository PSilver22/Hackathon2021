using System;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace EmailSender_ns
{
    
    public class EmailSender
    {
        public static string emailAddress;
        public static string password;
        public static NetworkCredential readInfo(string filepath)
        {
            string[] lines = System.IO.File.ReadAllLines(@filepath);
            return new NetworkCredential(lines[0], lines[1]);
        }

        public SmtpClient smtpClient = new("smtp.gmail.com")
        {
            Port = 587,
            Credentials = readInfo(@"C:\Users\a3210\Machon Lev\Hackathon\EmailSender\EmailInfo.txt"),
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
