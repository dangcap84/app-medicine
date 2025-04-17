namespace MediTrack.Domain.Exceptions.Domain
{
    public class UnauthorizedResourceAccessException : BaseException
    {
        public UnauthorizedResourceAccessException(string resourceType, object id) 
            : base($"Access to {resourceType} with id '{id}' is not authorized", "DOM002")
        {
            ResourceType = resourceType;
            ResourceId = id;
        }

        public string ResourceType { get; }
        public object ResourceId { get; }
    }
}
