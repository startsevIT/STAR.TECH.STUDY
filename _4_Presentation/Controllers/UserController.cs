using Application.Users;
using Application.Users.Models;
using Microsoft.AspNetCore.Mvc;
using Swagger.CustomAPIDoc;

namespace Presentation.Controllers;

public class UserController(UserService userService) : ControllerBase
{
    [HttpPost("register")]
    [ForReactInnerClient]
    [ForFlutterOuterClient]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        try
        {
            await userService.Register(request);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
        
        
    }

    [HttpPost("login")]
    [ForReactInnerClient]
    [ForFlutterOuterClient]
    public IActionResult Login([FromBody] UserLoginRequest request)
    {
        var response = userService.Login(request);
        return Ok(response);
    }
}