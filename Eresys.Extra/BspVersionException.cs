using System;
using System.Runtime.Serialization;

namespace Eresys.Extra
{
    [Serializable]
    public class BspVersionException : Exception
    {
        public string BspFileName { get; set; }

        public BspVersionException()
        { }

        public BspVersionException(string bspFileName)
            : base($"Version of {bspFileName} is not supported!")
        {
            BspFileName = bspFileName;
        }

        public BspVersionException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected BspVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
