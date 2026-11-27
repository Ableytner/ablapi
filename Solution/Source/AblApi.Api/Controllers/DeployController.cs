using AblApi.Common.ControllerDtos;
using AblApi.Core.AppK8s;
using AblApi.Core.AppJwtToken.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AblApi.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class DeployController(IK8sService k8sService) : ControllerBase
{
    private readonly IK8sService _k8sService = k8sService;

    [HttpPost]
    [AuthorizeDeploy]
    [EnableRateLimiting("deploy")]
    [EndpointSummary("Trigger a rolling restart of an allowlisted deployment")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Deploy([FromBody] DeployRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return BadRequest("A deployment 'name' is required.");
        }

        try
        {
            await _k8sService.RestartDeploymentAsync(request.Name, cancellationToken);
            return Accepted(new { message = $"Rolling restart of '{request.Name}' accepted." });
        }
        catch (KeyNotFoundException)
        {
            return BadRequest($"Deployment target '{request.Name}' is not allowed.");
        }
    }
}
