using CompanyManagement.Api.Data;
using CompanyManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CompanyManagement.Api.Repositories;

public class OrdenRepository : IOrdenRepository
{
    private readonly AppDbContext _context;

    public OrdenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Orden>> GetAllAsync()
    {
        return await _context.Ordenes
            .AsNoTracking()
            .Include(o => o.Cliente)
            .OrderByDescending(o => o.FechaOrden)
            .ToListAsync();
    }

    public async Task<Orden?> GetByIdAsync(int id)
    {
        return await _context.Ordenes
            .AsNoTracking()
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Orden>> GetByClienteIdAsync(int clienteId)
    {
        return await _context.Ordenes
            .AsNoTracking()
            .Include(o => o.Cliente)
            .Where(o => o.ClienteId == clienteId)
            .OrderByDescending(o => o.FechaOrden)
            .ToListAsync();
    }

    public async Task<Orden> AddAsync(Orden orden)
    {
        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();
        await _context.Entry(orden).Reference(o => o.Cliente).LoadAsync();
        return orden;
    }
}
