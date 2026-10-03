using Application.Organisations;
using FromToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

public class OrganisationController(OrganisationService organisationService) : ControllerBase
{
    [HttpPost, Authorize]
    public IActionResult Create([FromToken] Guid userId, [FromBody] OrganisationCreateRequest request) =>
        Ok(organisationService.RegisterOrganisation(userId, request));
}