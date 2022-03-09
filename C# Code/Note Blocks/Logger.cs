using System;
using System.IO;

namespace Note_Blocks
{
    public class Logger
    {
        public enum LogImportance
        {
            Debug,
            Information,
            Warning,
            Error,
            Fatal,
            Generic
        }
        public enum LogLevel
        {
            Debug,
            Information,
            Warning,
            None
        }
        public LogLevel ConsoleLevel { get; set; }
        public LogLevel FileLevel { get; set; }
        public string FilePath { get; }
        public Logger(string path)
        {
            ConsoleLevel = LogLevel.Information;
            FileLevel = LogLevel.Information;
            FilePath = path;
            File.WriteAllText(path, null);
        }
        public void Log(string log, LogImportance level)
        {
            string line = null;
            switch(level)
            {
                case LogImportance.Debug:
                    line = $"[Debug] {log}";
                    if (ConsoleLevel == LogLevel.Debug)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    if (FileLevel == LogLevel.Debug) File.AppendAllText(FilePath, $"\n{line}");
                    break;
                case LogImportance.Information:
                    line = $"[Info] {log}";
                    if (ConsoleLevel <= LogLevel.Information) Console.WriteLine(line);
                    if (FileLevel <= LogLevel.Information) File.AppendAllText(FilePath, $"\n{line}");
                    break;
                case LogImportance.Warning:
                    line = $"[Warning] {log}";
                    if (ConsoleLevel <= LogLevel.Warning)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    if (FileLevel <= LogLevel.Warning) File.AppendAllText(FilePath, $"\n{line}");
                    break;
                case LogImportance.Error:
                    line = $"[Error] {log}";
                    if (ConsoleLevel <= LogLevel.Warning)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    if (FileLevel <= LogLevel.Warning) File.AppendAllText(FilePath, $"\n{line}");
                    break;
                case LogImportance.Fatal:
                    line = $"[Fatal] {log}";
                    if (ConsoleLevel <= LogLevel.Warning)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(line);
                        Console.ResetColor();
                    }
                    if (FileLevel <= LogLevel.Warning) File.AppendAllText(FilePath, $"\n{line}");
                    break;
                case LogImportance.Generic:
                    line = log;
                    if (ConsoleLevel <= LogLevel.Warning) Console.WriteLine(line);
                    if (FileLevel <= LogLevel.Warning) File.AppendAllText(FilePath, $"\n{line}");
                    break;
            }
        }
    }
}