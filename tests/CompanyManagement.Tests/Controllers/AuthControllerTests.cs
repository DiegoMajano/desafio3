using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using CompanyManagement.Tests.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyManagement.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_UsuarioNuevo_SeAsignaRolUserYDevuelveToken()
    {
        // Arrange
        await using var provider = await TestFactory.CrearIdentityAsync();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var controller = new AuthController(userManager, TestFactory.CrearTokenService());

        // Act
        var resultado = await controller.Register(new RegisterRequest { Email = "nuevo@test.com", Password = "Clave123" });

        // Assert
        var objeto = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(201, objeto.StatusCode);
        var respuesta = Assert.IsType<AuthResponse>(objeto.Value);
        Assert.Contains(Roles.User, respuesta.Roles);

        var usuario = await userManager.FindByEmailAsync("nuevo@test.com");
        Assert.NotNull(usuario);
        Assert.True(await userManager.IsInRoleAsync(usuario!, Roles.User));
        Assert.False(await userManager.IsInRoleAsync(usuario!, Roles.Admin));
    }

    [Fact]
    public async Task Login_PasswordIncorrecto_DevuelveUnauthorized()
    {
        // Arrange
        await using var provider = await TestFactory.CrearIdentityAsync();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var controller = new AuthController(userManager, TestFactory.CrearTokenService());
        await controller.Register(new RegisterRequest { Email = "maria@test.com", Password = "Clave123" });

        // Act
        var resultado = await controller.Login(new LoginRequest { Email = "maria@test.com", Password = "otraClave1" });

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(resultado);
    }

    [Fact]
    public async Task Login_CredencialesCorrectas_DevuelveJwtConRol()
    {
        // Arrange
        await using var provider = await TestFactory.CrearIdentityAsync();
        using var scope = provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var admin = new IdentityUser { UserName = "admin@test.com", Email = "admin@test.com" };
        await userManager.CreateAsync(admin, "Admin123");
        await userManager.AddToRoleAsync(admin, Roles.Admin);
        var controller = new AuthController(userManager, TestFactory.CrearTokenService());

        // Act
        var resultado = await controller.Login(new LoginRequest { Email = "admin@test.com", Password = "Admin123" });

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var respuesta = Assert.IsType<AuthResponse>(ok.Value);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(respuesta.Token);
        Assert.Contains(jwt.Claims, c => (c.Type == "role" || c.Type == ClaimTypes.Role) && c.Value == Roles.Admin);
        Assert.True(respuesta.Expira > DateTime.UtcNow);
    }

    [Fact]
    public async Task Reportes_UsuarioConRolUser_NoEstaAutorizado_YAdminSi()
    {
        // Arrange
        await using var provider = await TestFactory.CrearIdentityAsync();
        var authorizationService = provider.GetRequiredService<IAuthorizationService>();

        var atributo = typeof(ReportesController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(atributo);
        var politica = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(atributo!.Roles!.Split(','))
            .Build();

        var usuarioNormal = CrearPrincipal("user@test.com", Roles.User);
        var administrador = CrearPrincipal("admin@test.com", Roles.Admin);

        // Act
        var resultadoUser = await authorizationService.AuthorizeAsync(usuarioNormal, null, politica);
        var resultadoAdmin = await authorizationService.AuthorizeAsync(administrador, null, politica);

        // Assert
        Assert.False(resultadoUser.Succeeded);
        Assert.True(resultadoAdmin.Succeeded);
    }

    [Fact]
    public void Endpoints_ClientesYOrdenesRequierenAutenticacion_YAuthEsPublico()
    {
        // Arrange
        var controladoresProtegidos = new[] { typeof(ClientesController), typeof(OrdenesController) };

        // Act
        var protegidos = controladoresProtegidos.All(t => t.GetCustomAttribute<AuthorizeAttribute>() is not null);
        var registroPublico = typeof(AuthController).GetMethod(nameof(AuthController.Register))!
            .GetCustomAttribute<AllowAnonymousAttribute>() is not null;
        var loginPublico = typeof(AuthController).GetMethod(nameof(AuthController.Login))!
            .GetCustomAttribute<AllowAnonymousAttribute>() is not null;

        // Assert
        Assert.True(protegidos);
        Assert.True(registroPublico);
        Assert.True(loginPublico);
    }

    private static ClaimsPrincipal CrearPrincipal(string email, string rol)
    {
        var identidad = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, rol)
        }, "TestAuth");

        return new ClaimsPrincipal(identidad);
    }
}
