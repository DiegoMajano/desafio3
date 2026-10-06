using CompanyManagement.Api.Data;
using CompanyManagement.Api.Models;
using CompanyManagement.Api.Services;
using CompanyManagement.Api.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CompanyManagement.Tests.Helpers;

public static class TestFactory
{
    public static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"CompanyManagementTests_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    public static IDistributedCache CrearCacheDistribuida() =>
        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    public static ICacheService CrearCacheService(IDistributedCache? cache = null) =>
        new RedisCacheService(
            cache ?? CrearCacheDistribuida(),
            Options.Create(new CacheSettings { ExpirationMinutes = 10 }),
            NullLogger<RedisCacheService>.Instance);

    public static ITokenService CrearTokenService() =>
        new TokenService(Options.Create(new JwtSettings
        {
            Key = "clave-de-pruebas-unitarias-con-mas-de-32-caracteres",
            Issuer = "CompanyManagement.Tests",
            Audience = "CompanyManagement.Tests",
            ExpirationMinutes = 30
        }));

    public static async Task<ServiceProvider> CrearIdentityAsync()
    {
        var nombreDb = $"IdentityTests_{Guid.NewGuid()}";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(nombreDb));
        services.AddIdentityCore<IdentityUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 6;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        services.AddAuthorization();

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in Roles.Todos)
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }

        return provider;
    }

    public static async Task<Cliente> AgregarClienteAsync(AppDbContext context, string nombre, string email, params decimal[] montos)
    {
        var cliente = new Cliente
        {
            Nombre = nombre,
            Email = email,
            FechaRegistro = DateTime.UtcNow.AddDays(-5)
        };

        foreach (var monto in montos)
        {
            cliente.Ordenes.Add(new Orden { FechaOrden = DateTime.UtcNow, MontoTotal = monto });
        }

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return cliente;
    }
}
