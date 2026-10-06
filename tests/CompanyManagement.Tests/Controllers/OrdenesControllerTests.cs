using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.Data;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Repositories;
using CompanyManagement.Api.Services;
using CompanyManagement.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CompanyManagement.Tests.Controllers;

public class OrdenesControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _cacheDistribuida;
    private readonly OrdenesController _controller;

    public OrdenesControllerTests()
    {
        _context = TestFactory.CrearContexto();
        _cacheDistribuida = TestFactory.CrearCacheDistribuida();
        _controller = new OrdenesController(
            new OrdenRepository(_context),
            new ClienteRepository(_context),
            TestFactory.CrearCacheService(_cacheDistribuida));
    }

    [Fact]
    public async Task Create_ClienteValido_DevuelveCreatedConLaOrden()
    {
        // Arrange
        var cliente = await TestFactory.AgregarClienteAsync(_context, "Hugo Méndez", "hugo@test.com");
        var orden = new OrdenCreateDto { ClienteId = cliente.Id, MontoTotal = 149.999m };

        // Act
        var resultado = await _controller.Create(orden);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var dto = Assert.IsType<OrdenDto>(created.Value);
        Assert.Equal(cliente.Id, dto.ClienteId);
        Assert.Equal("Hugo Méndez", dto.NombreCliente);
        Assert.Equal(150.00m, dto.MontoTotal);
    }

    [Fact]
    public async Task Create_ClienteInexistente_DevuelveBadRequest()
    {
        // Arrange
        var orden = new OrdenCreateDto { ClienteId = 404, MontoTotal = 20m };

        // Act
        var resultado = await _controller.Create(orden);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Empty(_context.Ordenes);
    }

    [Fact]
    public async Task GetAll_SegundaLlamada_SeResuelveDesdeCache()
    {
        // Arrange
        await TestFactory.AgregarClienteAsync(_context, "Irene Castro", "irene@test.com", 10m, 20m);
        await _controller.GetAll();

        // se borran las órdenes directamente en la base para comprobar que la respuesta viene de cache
        _context.Ordenes.RemoveRange(_context.Ordenes);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _controller.GetAll();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var ordenes = Assert.IsAssignableFrom<IEnumerable<OrdenDto>>(ok.Value);
        Assert.Equal(2, ordenes.Count());
    }

    [Fact]
    public async Task Create_InvalidaElListadoDeOrdenesEnCache()
    {
        // Arrange
        var cliente = await TestFactory.AgregarClienteAsync(_context, "Jorge Lara", "jorge@test.com", 5m);
        await _controller.GetAll();
        Assert.NotNull(await _cacheDistribuida.GetStringAsync(CacheKeys.Ordenes));

        // Act
        await _controller.Create(new OrdenCreateDto { ClienteId = cliente.Id, MontoTotal = 30m });
        var resultado = await _controller.GetAll();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<OrdenDto>>(ok.Value).Count());
    }

    [Fact]
    public async Task GetByCliente_DevuelveSoloLasOrdenesDelCliente()
    {
        // Arrange
        var karla = await TestFactory.AgregarClienteAsync(_context, "Karla Vega", "karla@test.com", 10m, 15m, 20m);
        await TestFactory.AgregarClienteAsync(_context, "Luis Ortiz", "luis@test.com", 99m);

        // Act
        var resultado = await _controller.GetByCliente(karla.Id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var ordenes = Assert.IsAssignableFrom<IEnumerable<OrdenDto>>(ok.Value).ToList();
        Assert.Equal(3, ordenes.Count);
        Assert.All(ordenes, o => Assert.Equal(karla.Id, o.ClienteId));
    }

    [Fact]
    public async Task GetByCliente_ClienteInexistente_DevuelveNotFound()
    {
        // Act
        var resultado = await _controller.GetByCliente(77);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task GetById_OrdenInexistente_DevuelveNotFound()
    {
        // Act
        var resultado = await _controller.GetById(500);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado.Result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
