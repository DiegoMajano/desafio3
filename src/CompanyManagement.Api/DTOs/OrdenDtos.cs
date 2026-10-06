using System.ComponentModel.DataAnnotations;

namespace CompanyManagement.Api.DTOs;

public record OrdenDto(int Id, int ClienteId, string NombreCliente, DateTime FechaOrden, decimal MontoTotal);

public class OrdenCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El ClienteId no es válido")]
    public int ClienteId { get; set; }

    public DateTime? FechaOrden { get; set; }

    [Range(0.01, 9999999999999.99, ErrorMessage = "El monto debe ser mayor que cero")]
    public decimal MontoTotal { get; set; }
}
