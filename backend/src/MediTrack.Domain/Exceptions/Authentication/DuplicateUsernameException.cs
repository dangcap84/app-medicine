namespace MediTrack.Domain.Exceptions.Authentication
{
    public class DuplicateUsernameException : BaseException
    {
        public DuplicateUsernameException(string username) 
            : base($"Username '{username}' already exists", "AUTH002")
        {
            Username = username;
        }

        public string Username { get; }
    }
}
