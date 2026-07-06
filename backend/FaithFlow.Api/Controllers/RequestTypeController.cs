using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Backend.DTOs;
using FaithFlow.Backend.Interfaces;

namespace FaithFlow.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RequestTypeController : ControllerBase
{
    private readonly IRequestTypeRepository _requestTypeService;

    public RequestTypeController(IRequestTypeRepository requestTypeService)
    {
        _requestTypeService = requestTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequestTypeDto>>> GetAll()
    {
        var types = await _requestTypeService.GetAllAsync();

        var dtos = types.Select(rt => new RequestTypeDto
        {
            Id = rt.Id,
            Name = rt.Name,
        });

        return Ok(dtos);
    }
}
