using AblApi.Core.AppJwtToken.Attributes;
using AblApi.Core.AppWillhaben;
using AblApi.Core.AppWillhaben.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("[controller]/")]
[ApiController]
public class WillhabenController(IWillhabenService willhabenService) : ControllerBase
{
    private readonly IWillhabenService _willhabenService = willhabenService;

    [HttpPost("config")]
    [AuthorizeWillhabenConfig]
    [EndpointSummary("Create a new Willhaben config")]
    [ProducesResponseType(typeof(WillhabenConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WillhabenConfigDto>> CreateConfig([FromBody] WillhabenConfigDto dto)
    {
        var result = await _willhabenService.CreateConfigAsync(dto);
        return CreatedAtAction(nameof(CreateConfig), result);
    }

    [HttpPost("config/{name}")]
    [AuthorizeWillhabenConfig]
    [EndpointSummary("Update an existing Willhaben config")]
    [ProducesResponseType(typeof(WillhabenConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WillhabenConfigDto>> UpdateConfig(string name, [FromBody] WillhabenConfigDto dto)
    {
        var existing = await _willhabenService.ListConfigsAsync();
        if (!existing.Any(c => c.Name == name))
        {
            return NotFound();
        }

        var result = await _willhabenService.UpdateConfigAsync(dto);
        return Ok(result);
    }

    [HttpDelete("config/{name}")]
    [AuthorizeWillhabenConfig]
    [EndpointSummary("Delete a Willhaben config by name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfig(string name)
    {
        var existing = await _willhabenService.ListConfigsAsync();
        if (!existing.Any(c => c.Name == name))
        {
            return NotFound();
        }

        await _willhabenService.DeleteConfigAsync(name);
        return NoContent();
    }

    [HttpGet("config")]
    [AuthorizeWillhabenConfig]
    [EndpointSummary("List all Willhaben configs")]
    [ProducesResponseType(typeof(List<WillhabenConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WillhabenConfigDto>>> ListConfigs()
    {
        var configs = await _willhabenService.ListConfigsAsync();
        return Ok(configs);
    }
}
