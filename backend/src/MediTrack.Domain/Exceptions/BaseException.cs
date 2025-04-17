using System;

namespace MediTrack.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public string Code { get; }

        protected BaseException(string message, string code) : base(message)
        {
            Code = code;
        }

        protected BaseException(string message, string code, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }
    }
}
