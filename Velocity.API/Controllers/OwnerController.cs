using Microsoft.AspNetCore.Mvc;
using Velocity.API.Models;
using Velocity.API.Repositories;

namespace Velocity.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OwnerController : ControllerBase
{
    private readonly IOwnerRepository _ownerRepository;
    
    public OwnerController(IOwnerRepository ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }
    
    [HttpPut]
    [ProducesResponseType(typeof(Owner), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertOwner(Owner owner)
    {
        try
        {
            return Ok(await _ownerRepository.UpsertOwner(owner));
        }
        catch(Exception e)
        {
            return new ObjectResult(new ErrorResponse(e))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Owner), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOwnerById([FromRoute] Guid id)
    {
        return Ok(await _ownerRepository.GetOwnerById(id));
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Owner>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOwners([FromQuery] Guid? companyId = null)
    {
        return Ok(companyId != null && companyId != Guid.Empty
            ? await _ownerRepository.GetOwnersByCompany(companyId.Value)
            : await _ownerRepository.GetAllOwners());
    }
}