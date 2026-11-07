namespace AblApi.Core.AppSpotify.Exceptions;

[Serializable]
public class UserNotLinkedException : Exception
{
    public UserNotLinkedException ()
    {}

    public UserNotLinkedException (string message) 
        : base(message)
    {}

    public UserNotLinkedException (string message, Exception innerException)
        : base (message, innerException)
    {}    
}
