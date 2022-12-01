using System;
using System.Diagnostics;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace MessageSenderDotnet;

class Program
{
    static void Main(string[] args)
    {
        int messageTimeoutMs = 10000;
        string[] files;
        MessageReason messageReason;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        while (true)
        {
            files = Directory.GetFiles(@"MessageReasons");
            string file = string.Empty;
            messageReason = CheckMessageReason(files, out file);

            if (stopwatch.ElapsedMilliseconds > messageTimeoutMs && messageReason != MessageReason.NoReason)
            {
                SendMessage(messageReason);
                DeleteFile(file);
                stopwatch.Restart();
            }
        }
    }

    private static void DeleteFile(string filePath) => File.Delete(filePath);

    private static MessageReason CheckMessageReason(string[] files, out string filePath)
    {
        if (files.Contains(@"MessageReasons\Motion.txt"))
        {
            filePath = @"MessageReasons\Motion.txt";
            return MessageReason.Motion;
        }
        else if (files.Contains(@"MessageReasons\Fire.txt"))
        {
            filePath = @"MessageReasons\Fire.txt";
            return MessageReason.Fire;
        }
        else if (files.Contains(@"MessageReasons\Gas.txt"))
        {
            filePath = @"MessageReasons\Gas.txt";
            return MessageReason.Gas;
        }
        else if (files.Contains(@"MessageReasons\Humidity.txt"))
        {
            filePath = @"MessageReasons\Humidity.txt";
            return MessageReason.Humidity;
        }
        else
        {
            filePath = string.Empty;
            return MessageReason.NoReason;
        }
    }

    private static void SendMessage(MessageReason messageReason)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sergey Kozlov", "serezha-kozlov.2002@mail.ru"));
        message.To.Add(new MailboxAddress("Sergey Kozlov", "serezha-kozlov.2002@mail.ru"));
        message.Subject = messageReason.ToString() + " in your house";

        message.Body = new TextPart("plain")
        {
            Text = @$"Hey {message.To[0].Name},

I just wanted to let you know that something happening in your house and I think u must know about it?

MessageReason: {messageReason.ToString()}

-- ArduinoSignalization"
        };

        using (var client = new SmtpClient())
        {
            client.Connect("smtp.mail.ru", 465);
            // // Note: only needed if the SMTP server requires authentication
            client.Authenticate("serezha-kozlov.2002@mail.ru", "JQHcGvKXDhd9ZajUKi0X");
            client.Send(message);
            Console.WriteLine($"Message about {messageReason} was sent");
            messageReason = MessageReason.NoReason;
            client.Disconnect(true);
        }
    }
}

public enum MessageReason
{
    NoReason,
    Motion,
    Gas,
    Fire,
    Humidity
}