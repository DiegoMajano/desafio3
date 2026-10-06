using System.ComponentModel.DataAnnotations;

namespace CompanyManagement.Api.DTOs;

public record ClienteDto(int Id, string Nombre, string Email, DateTime FechaRegistro, int TotalOrdenes);

public class ClienteCreateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class ClienteUpdateDto : ClienteCreateDto
{
}
