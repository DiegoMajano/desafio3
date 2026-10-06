using CompanyManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CompanyManagement.Api.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Orden> Ordenes => Set<Orden>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(c => c.Email).IsUnique();
            entity.Property(c => c.FechaRegistro).HasColumnType("datetime2");
        });

        builder.Entity<Orden>(entity =>
        {
            entity.ToTable("Ordenes");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.FechaOrden).HasColumnType("datetime2");
            entity.Property(o => o.MontoTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(o => o.Cliente)
                  .WithMany(c => c.Ordenes)
                  .HasForeignKey(o => o.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
