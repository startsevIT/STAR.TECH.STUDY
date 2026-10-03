using Application.Organisations;
using FromToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swagger.CustomAPIDoc;
using OrganisationCreateRequest = Application.Organisations.Models.OrganisationCreateRequest;

namespace Presentation.Controllers;

public class OrganisationController(OrganisationService organisationService) : ControllerBase
{
    //todo БЭКОФФИС приложение this register can only staff manager, not owner
    [HttpPost, Authorize]
    [ForBackOfficeClient]
    public IActionResult Create([FromToken] Guid userId, [FromBody] OrganisationCreateRequest request) =>
        Ok(organisationService.RegisterOrganisation(userId, request));
}