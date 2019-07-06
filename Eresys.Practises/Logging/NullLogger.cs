using System;

namespace Eresys.Practises.Logging
{
    public class NullLogger : ILogger
    {
        public void Log(LogLevels level, string message)
        { }

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
