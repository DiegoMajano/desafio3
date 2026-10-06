using CompanyManagement.Api.Models;

namespace CompanyManagement.Api.Repositories;

public interface IOrdenRepository
{
    Task<List<Orden>> GetAllAsync();
    Task<Orden?> GetByIdAsync(int id);
    Task<List<Orden>> GetByClienteIdAsync(int clienteId);
    Task<Orden> AddAsync(Orden orden);
}
