namespace BillionSongs
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    public class LyricsGeneratorException : Exception
    {
        public LyricsGeneratorException() {
        }

        protected LyricsGeneratorException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LyricsGeneratorException(string message) : base(message) {
        }

        public LyricsGeneratorException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}