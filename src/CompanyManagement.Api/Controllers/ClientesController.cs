using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using CompanyManagement.Api.Repositories;
using CompanyManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManagement.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _repository;
    private readonly ICacheService _cache;

    public ClientesController(IClienteRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll()
    {
        var cacheados = await _cache.GetAsync<List<ClienteDto>>(CacheKeys.Clientes);
        if (cacheados is not null)
        {
            return Ok(cacheados);
        }

        var clientes = (await _repository.GetAllAsync()).Select(c => c.ToDto()).ToList();
        await _cache.SetAsync(CacheKeys.Clientes, clientes);
        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var cacheado = await _cache.GetAsync<ClienteDto>(CacheKeys.Cliente(id));
        if (cacheado is not null)
        {
            return Ok(cacheado);
        }

        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = $"No existe un cliente con Id {id}" });
        }

        var dto = cliente.ToDto();
        await _cache.SetAsync(CacheKeys.Cliente(id), dto);
        return Ok(dto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] ClienteCreateDto dto)
    {
        if (await _repository.EmailExistsAsync(dto.Email))
        {
            return Conflict(new { mensaje = "Ya existe un cliente con ese email" });
        }

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Email = dto.Email.Trim(),
            FechaRegistro = DateTime.UtcNow
        };

        await _repository.AddAsync(cliente);
        await _cache.RemoveAsync(CacheKeys.Clientes);

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente.ToDto());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] ClienteUpdateDto dto)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = $"No existe un cliente con Id {id}" });
        }

        if (await _repository.EmailExistsAsync(dto.Email, id))
        {
            return Conflict(new { mensaje = "Ya existe otro cliente con ese email" });
        }

        cliente.Nombre = dto.Nombre.Trim();
        cliente.Email = dto.Email.Trim();
        await _repository.UpdateAsync(cliente);

        // las órdenes muestran el nombre del cliente, por eso también se limpia ese listado
        await _cache.RemoveAsync(CacheKeys.Clientes, CacheKeys.Cliente(id), CacheKeys.Ordenes);

        return Ok(cliente.ToDto());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _repository.GetByIdAsync(id);
        if (cliente is null)
        {
            return NotFound(new { mensaje = $"No existe un cliente con Id {id}" });
        }

        await _repository.DeleteAsync(cliente);
        await _cache.RemoveAsync(CacheKeys.Clientes, CacheKeys.Cliente(id), CacheKeys.Ordenes);

        return NoContent();
    }
}
