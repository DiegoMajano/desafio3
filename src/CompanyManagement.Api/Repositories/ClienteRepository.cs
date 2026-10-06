using CompanyManagement.Api.Data;
using CompanyManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyManagement.Api.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> GetAllAsync()
    {
        return await _context.Clientes
            .AsNoTracking()
            .Include(c => c.Ordenes)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _context.Clientes
            .Include(c => c.Ordenes)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public Task<bool> ExistsAsync(int id)
    {
        return _context.Clientes.AnyAsync(c => c.Id == id);
    }

    public Task<bool> EmailExistsAsync(string email, int? excluirId = null)
    {
        var normalizado = email.Trim().ToLower();
        return _context.Clientes.AnyAsync(c => c.Email.ToLower() == normalizado
                                               && (excluirId == null || c.Id != excluirId));
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
    }
}
