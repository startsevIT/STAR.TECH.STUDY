using Application.Organisations;
using Application.Organisations.Models;
using Domain.Users;
using FromToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

public class OrganisationRegisterRequestController(
    OrganisationRegisterRequestService organisationRegisterRequestService) : Controller
{
    [HttpPost("{organisationId:guid}"), Authorize]
    public IActionResult Create([FromToken] Guid userId, Guid organisationId, [FromBody] Role role)
    {
        organisationRegisterRequestService.Create(userId, organisationId, role);
        return Ok();
    }

    [HttpPut("approve/{requestId:guid}"), Authorize]
    public IActionResult Approve(Guid requestId, [FromToken] Guid userId)
    {
        organisationRegisterRequestService.Approve(requestId, userId);
        return Ok();
    }

    [HttpPut("reject/{requestId:guid}"), Authorize]
    public IActionResult Reject(Guid requestId, [FromToken] Guid userId)
    {
        organisationRegisterRequestService.Reject(requestId, userId);
        return Ok();
    }

    [HttpGet("{organisationId:guid}"), Authorize]
    public ActionResult<OrganisationUserRegistrationRequestDto> Get(Guid organisationId, [FromToken] Guid userId)
    {
        return Ok(organisationRegisterRequestService.GetPending(organisationId, userId));
    }
}