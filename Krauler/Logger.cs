﻿using System;
using System.Globalization;
using System.IO;

namespace Krauler
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    ///     Synchronized logger.
    /// </summary>
    public sealed class Logger
    {
        public static volatile bool LogToConsole = true;
        public static Logger Instance = new();

        public string Stream { get; private set; } = "";

        public void Write(Exception ex)
        {
            WriteLine(ex.Message, LogLevel.Error);
        }

        public void Write(string message, LogLevel level = LogLevel.Info)
        {
            message = $"[{DateTime.Now.ToString(CultureInfo.CurrentCulture)} {level.ToString()}]: {message}";
            lock (Stream)
            {
                Stream += message;
            }

            if (!LogToConsole) return;
            Console.ForegroundColor = level switch
            {
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void WriteLine(string message, LogLevel level = LogLevel.Info)
        {
            message = $"[{DateTime.Now.ToString(CultureInfo.CurrentCulture)} {level.ToString()}]: {message}";
            lock (Stream)
            {
                Stream += message;
                Stream += '\n';
            }

            if (!LogToConsole) return;
            Console.ForegroundColor = level switch
            {
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => Console.ForegroundColor
            };
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Clear()
        {
            lock (Stream)
            {
                Stream = "";
            }
        }

        public void Flush()
        {
            if (!Directory.Exists(Config.LoggingDir)) Directory.CreateDirectory(Config.LoggingDir);

            var time = DateTime.Now;
            string file =
                $"{Config.LoggingDir}Protocol-{time.ToShortDateString().Replace('/', '-')}-{time.TimeOfDay.Hours}-{time.TimeOfDay.Minutes}-{time.TimeOfDay.Seconds}.txt";
            File.WriteAllText(file, Stream);
        }
    }
}