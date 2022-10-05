using Masuit.Tools.Models;
using System.Diagnostics;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BustabitKing;

internal static class Helper
{
    public static IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    public static void SendMail(string title, string content, string? screenshotPath = null)
    {
        var email = new Email {
            Username = config["emailUsername"],
            Password = config["emailPassword"],
            SmtpPort = config.GetValue<int>("SmtpPort"),
            SmtpServer = config["SmtpServer"],
            Subject = title,
            Body = content,
            Tos = config["emailTos"]
        };

        if (screenshotPath != null)
        {
            email.Attachments.Add(new Attachment(screenshotPath));
        }

        email.Send();
    }

    public static void Monitor(object? processName)
    {
        var proc = Process.GetProcessesByName(processName as string).Single();
        var hasSendEmail = false;
        while (true)
        {
            Console.WriteLine($"待监控进程已退出? {proc.HasExited}");
            if (proc.HasExited && !hasSendEmail)
            {
                SendMail("警告", $"程序{processName}已退出!");
            }
            Thread.Sleep(1000);
        }
    }
}