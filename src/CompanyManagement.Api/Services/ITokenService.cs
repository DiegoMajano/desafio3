using CompanyManagement.Api.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CompanyManagement.Api.Services;

public interface ITokenService
{
    AuthResponse CrearToken(IdentityUser usuario, IList<string> roles);
}
