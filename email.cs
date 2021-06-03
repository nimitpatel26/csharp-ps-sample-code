using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;


public class EmailSender
{


    public bool SendEmail()
    {

        bool success = false;

        try
        {
            string _smtpServer = "place smtpserver here";

            MailMessage message = new MailMessage();
            message.From = new MailAddress("email_sender@email.com");
            message.To.Add("email1@email.com,email2@gmail.com");

            message.CC.Add("email3@email.com,email4@gmail.com");
            message.Bcc.Add("email5@email.com,email6@gmail.com");

            
            message.Subject = "place email subject here";
            message.Body = "place email body here";



            Attachment data = new Attachment("place file name here", MediaTypeNames.Application.Octet);
            message.Attachments.Add(data);


            SmtpClient client = new SmtpClient(_smtpServer);


            client.EnableSsl = true;
            client.Send(message);
            success = true;

            client.Dispose();
        }
        catch (Exception ex)
        {
            // email wasn't sent successfully
        }


        return success;
    }

}
