namespace AblApi.Core.AppSpotify.Exceptions;

[Serializable]
public class UnsupportedTrackTypeException : Exception
{
    public UnsupportedTrackTypeException ()
    {}

    public UnsupportedTrackTypeException (string message) 
        : base(message)
    {}

    public UnsupportedTrackTypeException (string message, Exception innerException)
        : base (message, innerException)
    {}    
}
