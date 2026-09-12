using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Domain;

namespace AblApi.Core.AppJwtToken;

public interface IJwtTokenService
{
    public JwtToken CreateToken(Guid userId, IEnumerable<ApiAccessRole> roles);
}
