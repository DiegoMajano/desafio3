using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;

namespace CompanyManagement.Api.Services;

public static class Mapper
{
    public static ClienteDto ToDto(this Cliente c) =>
        new(c.Id, c.Nombre, c.Email, c.FechaRegistro, c.Ordenes?.Count ?? 0);

    public static OrdenDto ToDto(this Orden o) =>
        new(o.Id, o.ClienteId, o.Cliente?.Nombre ?? string.Empty, o.FechaOrden, o.MontoTotal);
}
