using System;

namespace Eresys.Practises.Logging
{
    public enum LogLevels
    {
        Info,
        Debug,
        Warning,
        Error,
    }

    public interface ILogger
    {
        ILogger WithException(Exception ex);

        ILogger WithField(string key, object value);

        void Log(LogLevels level, string message);
    }
}
