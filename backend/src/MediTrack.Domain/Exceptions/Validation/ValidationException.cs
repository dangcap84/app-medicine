using System.Collections.Generic;

namespace MediTrack.Domain.Exceptions.Validation
{
    public class ValidationException : BaseException
    {
        public ValidationException(IDictionary<string, string[]> failures)
            : base("One or more validation failures have occurred.", "VAL001")
        {
            Failures = failures;
        }

        public ValidationException(string propertyName, string error)
            : base($"Validation failed: {error}", "VAL001")
        {
            Failures = new Dictionary<string, string[]>
            {
                { propertyName, new[] { error } }
            };
        }

        public IDictionary<string, string[]> Failures { get; }
    }
}
