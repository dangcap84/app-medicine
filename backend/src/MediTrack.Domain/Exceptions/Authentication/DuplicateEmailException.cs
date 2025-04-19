namespace MediTrack.Domain.Exceptions.Authentication;

/// <summary>
/// Exception được ném ra khi phát hiện email đã tồn tại trong hệ thống
/// </summary>
public class DuplicateEmailException : BaseException
{
    private const string DefaultMessage = "Email đã được sử dụng";
    private const string ErrorCode = "DUPLICATE_EMAIL";

    public DuplicateEmailException() : base(DefaultMessage, ErrorCode)
    {
    }

    public DuplicateEmailException(string message) : base(message, ErrorCode)
    {
    }
}
