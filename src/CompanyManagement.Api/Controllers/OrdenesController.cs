using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using CompanyManagement.Api.Repositories;
using CompanyManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManagement.Api.Controllers;

[ApiController]
[Route("api/ordenes")]
[Authorize]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenRepository _ordenes;
    private readonly IClienteRepository _clientes;
    private readonly ICacheService _cache;

    public OrdenesController(IOrdenRepository ordenes, IClienteRepository clientes, ICacheService cache)
    {
        _ordenes = ordenes;
        _clientes = clientes;
        _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrdenDto>>> GetAll()
    {
        var cacheadas = await _cache.GetAsync<List<OrdenDto>>(CacheKeys.Ordenes);
        if (cacheadas is not null)
        {
            return Ok(cacheadas);
        }

        var ordenes = (await _ordenes.GetAllAsync()).Select(o => o.ToDto()).ToList();
        await _cache.SetAsync(CacheKeys.Ordenes, ordenes);
        return Ok(ordenes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdenDto>> GetById(int id)
    {
        var orden = await _ordenes.GetByIdAsync(id);
        if (orden is null)
        {
            return NotFound(new { mensaje = $"No existe una orden con Id {id}" });
        }

        return Ok(orden.ToDto());
    }

    [HttpGet("cliente/{clienteId:int}")]
    [ProducesResponseType(typeof(IEnumerable<OrdenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrdenDto>>> GetByCliente(int clienteId)
    {
        if (!await _clientes.ExistsAsync(clienteId))
        {
            return NotFound(new { mensaje = $"No existe un cliente con Id {clienteId}" });
        }

        var ordenes = await _ordenes.GetByClienteIdAsync(clienteId);
        return Ok(ordenes.Select(o => o.ToDto()));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrdenDto>> Create([FromBody] OrdenCreateDto dto)
    {
        if (!await _clientes.ExistsAsync(dto.ClienteId))
        {
            return BadRequest(new { mensaje = $"El cliente con Id {dto.ClienteId} no existe" });
        }

        var orden = new Orden
        {
            ClienteId = dto.ClienteId,
            FechaOrden = dto.FechaOrden ?? DateTime.UtcNow,
            MontoTotal = Math.Round(dto.MontoTotal, 2)
        };

        await _ordenes.AddAsync(orden);

        // el total de órdenes del cliente cambia, así que también se limpia su cache
        await _cache.RemoveAsync(CacheKeys.Ordenes, CacheKeys.Clientes, CacheKeys.Cliente(dto.ClienteId));

        return CreatedAtAction(nameof(GetById), new { id = orden.Id }, orden.ToDto());
    }
}
