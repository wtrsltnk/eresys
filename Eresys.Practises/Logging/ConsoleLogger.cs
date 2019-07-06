using System;

namespace Eresys.Practises.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(LogLevels level, string message)
        {
            Console.WriteLine($"[{Enum.GetName(typeof(LogLevels), level)}] : {message}");
        }

        public ILogger WithException(Exception ex)
        {
            return this;
        }

        public ILogger WithField(string key, object value)
        {
            return this;
        }
    }
}
