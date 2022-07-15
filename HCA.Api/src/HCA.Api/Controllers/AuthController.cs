using HCA.Api.Dto;
using HCA.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace HCA.Api.Controllers;

[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [Route("/login")]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserCredentials userCredentials)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _authenticationService.CreateAccessTokenAsync(userCredentials.Email, userCredentials.Password);

        if (null == response)
        {
            return Unauthorized("Invalid User Name or Password");
        }

        return Ok(response);
    }
}

