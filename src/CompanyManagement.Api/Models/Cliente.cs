namespace CompanyManagement.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
