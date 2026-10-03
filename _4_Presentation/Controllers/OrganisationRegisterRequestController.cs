using Application.Organisations;
using Application.Organisations.Models;
using Domain.Users;
using FromToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swagger.CustomAPIDoc;

namespace Presentation.Controllers;

public class OrganisationRegisterRequestController(
    OrganisationRegisterRequestService organisationRegisterRequestService) : Controller
{
    //todo ВНЕШНЕЕ приложение. Клиент моежет подать заявку на вступление в организацию как препод или студент
    [HttpPost("{organisationId:guid}"), Authorize]
    [ForFlutterOuterClient]
    public IActionResult Create([FromToken] Guid userId, Guid organisationId, [FromBody] Role role)
    {
        organisationRegisterRequestService.Create(userId, organisationId, role);
        return Ok();
    }

    //todo ВНУТРЕННЕЕ приложение. Администрация организации может одобрить заявку на вступление в организацию
    [HttpPut("approve/{requestId:guid}"), Authorize]
    [ForReactInnerClient]
    public IActionResult Approve(Guid requestId, [FromToken] Guid userId)
    {
        organisationRegisterRequestService.Approve(requestId, userId);
        return Ok();
    }

    //todo ВНУТРЕННЕЕ приложение. Администрация организации может отклонить заявку на вступление в организацию
    [HttpPut("reject/{requestId:guid}"), Authorize]
    [ForReactInnerClient]
    public IActionResult Reject(Guid requestId, [FromToken] Guid userId)
    {
        organisationRegisterRequestService.Reject(requestId, userId);
        return Ok();
    }

    //todo ВНУТРЕННЕЕ приложение. Администрация организации может посмотреть заявки на вступление в конкретную организацию
    [HttpGet("{organisationId:guid}"), Authorize]
    [ForReactInnerClient]
    public ActionResult<OrganisationUserRegistrationRequestDto> Get(Guid organisationId, [FromToken] Guid userId)
    {
        return Ok(organisationRegisterRequestService.GetPending(organisationId, userId));
    }
}