using System.Security.Claims;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using CompanyManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Conflict(new { mensaje = "Ya existe un usuario registrado con ese email" });
        }

        var usuario = new IdentityUser { UserName = request.Email, Email = request.Email };
        var resultado = await _userManager.CreateAsync(usuario, request.Password);
        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No se pudo registrar el usuario",
                errores = resultado.Errors.Select(e => e.Description)
            });
        }

        await _userManager.AddToRoleAsync(usuario, Roles.User);

        var roles = await _userManager.GetRolesAsync(usuario);
        return StatusCode(StatusCodes.Status201Created, _tokenService.CrearToken(usuario, roles));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario is null || !await _userManager.CheckPasswordAsync(usuario, request.Password))
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas" });
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        return Ok(_tokenService.CrearToken(usuario, roles));
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.FindFirstValue(ClaimTypes.Name),
            roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
        });
    }
}
