using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.Data;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Repositories;
using CompanyManagement.Api.Services;
using CompanyManagement.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CompanyManagement.Tests.Controllers;

public class ClientesControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cacheDistribuida;
    private readonly ClientesController _controller;

    public ClientesControllerTests()
    {
        _context = TestFactory.CrearContexto();
        _cacheDistribuida = TestFactory.CrearCacheDistribuida();
        _controller = new ClientesController(
            new ClienteRepository(_context),
            TestFactory.CrearCacheService(_cacheDistribuida));
    }

    [Fact]
    public async Task GetAll_ConClientesRegistrados_DevuelveListaYLaGuardaEnCache()
    {
        // Arrange
        await TestFactory.AgregarClienteAsync(_context, "Ana López", "ana@test.com", 50m, 25m);
        await TestFactory.AgregarClienteAsync(_context, "Bruno Díaz", "bruno@test.com");

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var clientes = Assert.IsAssignableFrom<IEnumerable<ClienteDto>>(ok.Value).ToList();
        Assert.Equal(2, clientes.Count);
        Assert.Equal(2, clientes.Single(c => c.Email == "ana@test.com").TotalOrdenes);
        Assert.NotNull(await _cacheDistribuida.GetStringAsync(CacheKeys.Clientes));
    }

    [Fact]
    public async Task GetById_ClienteInexistente_DevuelveNotFound()
    {
        // Act
        var resultado = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task GetById_ClienteExistente_DevuelveCliente()
    {
        // Arrange
        var cliente = await TestFactory.AgregarClienteAsync(_context, "Carla Ruiz", "carla@test.com", 10m);

        // Act
        var resultado = await _controller.GetById(cliente.Id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ClienteDto>(ok.Value);
        Assert.Equal("Carla Ruiz", dto.Nombre);
        Assert.Equal(1, dto.TotalOrdenes);
    }

    [Fact]
    public async Task Create_DatosValidos_DevuelveCreatedEInvalidaCache()
    {
        // Arrange
        await _controller.GetAll();
        Assert.NotNull(await _cacheDistribuida.GetStringAsync(CacheKeys.Clientes));
        var nuevo = new ClienteCreateDto { Nombre = "Diego Martínez", Email = "diego@test.com" };

        // Act
        var resultado = await _controller.Create(nuevo);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var dto = Assert.IsType<ClienteDto>(created.Value);
        Assert.True(dto.Id > 0);
        Assert.Single(_context.Clientes);
        Assert.Null(await _cacheDistribuida.GetStringAsync(CacheKeys.Clientes));
    }

    [Fact]
    public async Task Create_EmailDuplicado_DevuelveConflict()
    {
        // Arrange
        await TestFactory.AgregarClienteAsync(_context, "Elena Torres", "elena@test.com");
        var duplicado = new ClienteCreateDto { Nombre = "Otra Elena", Email = "ELENA@test.com" };

        // Act
        var resultado = await _controller.Create(duplicado);

        // Assert
        Assert.IsType<ConflictObjectResult>(resultado.Result);
        Assert.Single(_context.Clientes);
    }

    [Fact]
    public async Task Update_ClienteExistente_ActualizaDatos()
    {
        // Arrange
        var cliente = await TestFactory.AgregarClienteAsync(_context, "Fernando Paz", "fernando@test.com");
        var cambios = new ClienteUpdateDto { Nombre = "Fernando Paz Molina", Email = "fpaz@test.com" };

        // Act
        var resultado = await _controller.Update(cliente.Id, cambios);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var dto = Assert.IsType<ClienteDto>(ok.Value);
        Assert.Equal("Fernando Paz Molina", dto.Nombre);
        Assert.Equal("fpaz@test.com", _context.Clientes.Single().Email);
    }

    [Fact]
    public async Task Update_ClienteInexistente_DevuelveNotFound()
    {
        // Act
        var resultado = await _controller.Update(123, new ClienteUpdateDto { Nombre = "Nadie", Email = "nadie@test.com" });

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Delete_ClienteExistente_EliminaClienteYSusOrdenes()
    {
        // Arrange
        var cliente = await TestFactory.AgregarClienteAsync(_context, "Gabriela Ríos", "gabriela@test.com", 100m);
        await _controller.GetById(cliente.Id);

        // Act
        var resultado = await _controller.Delete(cliente.Id);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
        Assert.Empty(_context.Clientes);
        Assert.Empty(_context.Ordenes);
        Assert.Null(await _cacheDistribuida.GetStringAsync(CacheKeys.Cliente(cliente.Id)));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
