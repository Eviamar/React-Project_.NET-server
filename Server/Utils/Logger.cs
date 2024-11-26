using System;
using System.IO;

namespace Server.Utils
{
    public static class Logger
    {
        private static readonly string LogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static string GetLogFilePath(string logType) =>
            Path.Combine(LogsDirectory, $"{logType}_{DateTime.UtcNow:yyyy-MM-dd}.txt");

        static Logger()
        {
            Directory.CreateDirectory(LogsDirectory); // Ensure the logs directory exists
        }

        public static void Log(string message)
        {
            string logMessage = $"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {message}";
            Console.WriteLine(logMessage);
            File.AppendAllText(GetLogFilePath("server"), logMessage + Environment.NewLine);
        }

        public static void LogLogin(string username, string ip, string userAgent)
        {
            string message = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}: User '{username}' logged in from IP '{ip}', using '{userAgent}'.";
            File.AppendAllText(GetLogFilePath("login"), message + Environment.NewLine);
        }

        public static void LogRegister(string username, string ip, string userAgent)
        {
            string message = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}: User '{username}' registered from IP '{ip}', using '{userAgent}'.";
            File.AppendAllText(GetLogFilePath("register"), message + Environment.NewLine);
        }

        public static void ErrorUserLog(Exception error, string username, string ip, string userAgent)
        {
            string message = $"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}: Error for user '{username}' from IP '{ip}' using '{userAgent}'.";

            // Check if an exception is provided and log its details
            if (error != null)
            {
                message += $"\nException: {error.Message}\nStack Trace:\n{error.StackTrace}";
            }
            else
            {
                message += "\nNo exception details provided.";
            }

            // Write the log to the user error log file
            File.AppendAllText(GetLogFilePath("user_errors"), message + Environment.NewLine);
        }
    }
}