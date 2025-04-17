namespace MediTrack.Domain.Exceptions.Domain
{
    public class ResourceNotFoundException : BaseException
    {
        public ResourceNotFoundException(string resourceType, object id) 
            : base($"{resourceType} with id '{id}' was not found", "DOM001")
        {
            ResourceType = resourceType;
            ResourceId = id;
        }

        public string ResourceType { get; }
        public object ResourceId { get; }
    }
}
