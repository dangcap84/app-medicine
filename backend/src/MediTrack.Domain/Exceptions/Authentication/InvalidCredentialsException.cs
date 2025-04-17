namespace MediTrack.Domain.Exceptions.Authentication
{
    public class InvalidCredentialsException : BaseException
    {
        public InvalidCredentialsException() : base("Invalid username or password", "AUTH001")
        {
        }
    }
}
