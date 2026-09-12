using AblApi.Common.Enums;

namespace AblApi.Common.Attributes;

public class AccessLevelAttribute(AccessLevelType type) : Attribute
{
    private readonly AccessLevelType Type = type;
}
