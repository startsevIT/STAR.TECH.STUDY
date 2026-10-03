using Application.Users;
using Microsoft.AspNetCore.Mvc;
using Swagger.CustomAPIDoc;

namespace Presentation.Controllers;

public class EmailConfirmationController(EmailConfirmationService emailConfirmationService) : ControllerBase
{
    [HttpGet("confirm-email")]
    [Produces("text/html")]
    [ForOnlyApi]
    public IActionResult ConfirmEmail([FromQuery] string token)
    {
        try
        {
            emailConfirmationService.Confirm(token);

            return Content(
                """
                <!DOCTYPE html>
                <html>
                <head><title>Email confirmed</title></head>
                <body>
                    <h1>Email confirmed</h1>
                    <p>You can now log in to the application.</p>
                </body>
                </html>
                """,
                "text/html");
        }
        catch (Exception)
        {
            return Content(
                """
                <!DOCTYPE html>
                <html>
                <head><title>Confirmation failed</title></head>
                <body>
                    <h1>Confirmation failed</h1>
                    <p>The link is invalid or expired.</p>
                </body>
                </html>
                """,
                "text/html");
        }
    }

    [HttpPost("resend-confirmation/{userId:guid}")]
    [ForOnlyApi]
    public IActionResult ResendConfirmation(Guid userId)
    {
        _ = emailConfirmationService.ResendAsync(userId);
        return NoContent();
    }
}