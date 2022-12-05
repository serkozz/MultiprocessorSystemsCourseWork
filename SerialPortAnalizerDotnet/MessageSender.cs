using System.Diagnostics;

using MailKit.Net.Smtp;
using MimeKit;

public class MessageSender
{
    public int MessageTimeoutMs { get; set; } = 60000;
    private Stopwatch stopwatch = new Stopwatch(); 
    private SensorsInfo sensorsInfo;
    private MessageReason messageReason = MessageReason.NoReason;
    private bool isActive = false;

    public MessageSender(int messageTimeoutMs, SensorsInfo sensorsInfo)
    {
        this.sensorsInfo = sensorsInfo;
        MessageTimeoutMs = messageTimeoutMs;
    }

    public void StartMonitoring()
    {
        stopwatch.Start();
        isActive = true;

        while (isActive)
        {
            messageReason = CheckMessageReason();

            if (stopwatch.ElapsedMilliseconds > MessageTimeoutMs && messageReason != MessageReason.NoReason)
            {
                SendMessage(messageReason);
                stopwatch.Restart();
            }
        }
    }

    private MessageReason CheckMessageReason()
    {
        if (sensorsInfo.Distance < SensorsThresholds.DISTANCE_SENSOR_THRESHOLD_IN_CM && sensorsInfo.Distance != 0)
            return MessageReason.Motion;
        else if (sensorsInfo.Humidity > SensorsThresholds.HUMIDITY_SENSOR_THRESHOLD_IN_PERCENTS && sensorsInfo.Humidity != 0)
            return MessageReason.Humidity;
        else if (sensorsInfo.Temperature > SensorsThresholds.TEMPERATURE_SENSOR_THRESHOLD_IN_CELSIUS && sensorsInfo.Temperature != 0)
            return MessageReason.Temperature;
        else if (sensorsInfo.GasSensorValue > SensorsThresholds.GAS_SENSOR_ANALOG_THRESHOLD && sensorsInfo.GasSensorValue != 0)
            return MessageReason.Gas;
        else
            return MessageReason.NoReason;
    }

    public void StopMonitoring()
    {
        isActive = false;
    }

    private void SendMessage(MessageReason messageReason)
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