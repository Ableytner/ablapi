namespace AblApi.Common.Enums;

public enum AccessLevelType
{
    /// <summary>
    ///		Functionality is accessible from the public internet.
    /// </summary>
    PublicInternetAccess = 0,

    /// <summary>
    ///		Functionality is accessible from any Account (everyone can create an Account).
    /// </summary>
    PublicAccountAccess = 1,
}
