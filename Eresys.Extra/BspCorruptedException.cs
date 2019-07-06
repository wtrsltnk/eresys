using System;
using System.Runtime.Serialization;

namespace Eresys.Extra
{
    [Serializable]
    public class BspCorruptedException : Exception
    {
        public string BspFileName { get; set; }

        public BspCorruptedException()
        { }

        public BspCorruptedException(string bspFileName)
            : base($"Contents of {bspFileName} is corrupted!")
        {
            BspFileName = bspFileName;
        }

        public BspCorruptedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected BspCorruptedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
