using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class DatosSemilla : Migration
    {
        // Las fechas se calculan desde el día en que se aplica la migración,
        // así el reporte de clientes inactivos (último mes) siempre tiene datos.
        private static readonly (string Nombre, string Email, int DiasRegistro)[] Clientes =
        {
            ("Distribuidora La Ceiba", "compras@laceiba.com.sv", 180),
            ("Ferretería El Martillo", "ventas@elmartillo.com.sv", 150),
            ("Panadería San Antonio", "pedidos@sanantonio.com.sv", 140),
            ("Farmacia Santa Lucía", "admin@farmaciasl.com.sv", 120),
            ("Café Los Volcanes", "info@cafelosvolcanes.com", 110),
            ("Librería Cuscatlán", "contacto@libcuscatlan.com", 95),
            ("Supermercado La Económica", "compras@laeconomica.com.sv", 90),
            ("Taller Mecánico Hermanos Paz", "taller@hermanospaz.com", 75),
            ("Clínica Dental Sonrisas", "citas@sonrisas.com.sv", 60),
            ("Imprenta Gráficos del Valle", "ventas@graficosdelvalle.com", 45),
            ("Agroservicio El Rancho", "agro@elrancho.com.sv", 25),
            ("Óptica Visión Clara", "citas@visionclara.com.sv", 18),
            ("Pupusería Doña Tere", "donatere@gmail.com", 10),
            ("Consultora Bosco & Asociados", "info@boscoasociados.com", 5),
            ("Floristería Jardín de Abril", "pedidos@jardindeabril.com", 2)
        };

        private static readonly (string Email, int DiasOrden, decimal Monto)[] Ordenes =
        {
            ("compras@laceiba.com.sv", 170, 1250.00m),
            ("compras@laceiba.com.sv", 130, 830.50m),
            ("compras@laceiba.com.sv", 85, 1975.00m),
            ("compras@laceiba.com.sv", 40, 2140.75m),
            ("compras@laceiba.com.sv", 6, 1120.30m),
            ("ventas@elmartillo.com.sv", 140, 455.20m),
            ("ventas@elmartillo.com.sv", 70, 610.00m),
            ("ventas@elmartillo.com.sv", 12, 389.90m),
            ("pedidos@sanantonio.com.sv", 120, 189.99m),
            ("pedidos@sanantonio.com.sv", 50, 245.50m),
            ("admin@farmaciasl.com.sv", 115, 3200.00m),
            ("admin@farmaciasl.com.sv", 88, 1475.40m),
            ("admin@farmaciasl.com.sv", 60, 2890.00m),
            ("admin@farmaciasl.com.sv", 33, 980.00m),
            ("admin@farmaciasl.com.sv", 9, 215.60m),
            ("admin@farmaciasl.com.sv", 3, 1340.25m),
            ("info@cafelosvolcanes.com", 100, 540.00m),
            ("info@cafelosvolcanes.com", 20, 712.80m),
            ("contacto@libcuscatlan.com", 80, 365.00m),
            ("compras@laeconomica.com.sv", 85, 4520.00m),
            ("compras@laeconomica.com.sv", 55, 3875.60m),
            ("compras@laeconomica.com.sv", 28, 5110.00m),
            ("compras@laeconomica.com.sv", 4, 2950.45m),
            ("taller@hermanospaz.com", 65, 725.35m),
            ("taller@hermanospaz.com", 15, 1180.00m),
            ("citas@sonrisas.com.sv", 45, 860.00m),
            ("ventas@graficosdelvalle.com", 30, 299.99m),
            ("ventas@graficosdelvalle.com", 7, 1450.00m)
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var c in Clientes)
            {
                migrationBuilder.Sql($@"
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Email = N'{c.Email}')
    INSERT INTO dbo.Clientes (Nombre, Email, FechaRegistro)
    VALUES (N'{Texto(c.Nombre)}', N'{c.Email}', DATEADD(DAY, -{c.DiasRegistro}, GETUTCDATE()));");
            }

            foreach (var o in Ordenes)
            {
                var monto = o.Monto.ToString(System.Globalization.CultureInfo.InvariantCulture);
                migrationBuilder.Sql($@"
INSERT INTO dbo.Ordenes (ClienteId, FechaOrden, MontoTotal)
SELECT c.Id, DATEADD(DAY, -{o.DiasOrden}, GETUTCDATE()), {monto}
FROM dbo.Clientes c
WHERE c.Email = N'{o.Email}'
  AND NOT EXISTS (SELECT 1 FROM dbo.Ordenes x
                  WHERE x.ClienteId = c.Id AND x.MontoTotal = {monto});");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var emails = string.Join(", ", Clientes.Select(c => $"N'{c.Email}'"));

            // las órdenes se borran en cascada junto con el cliente
            migrationBuilder.Sql($"DELETE FROM dbo.Clientes WHERE Email IN ({emails});");
        }

        private static string Texto(string valor) => valor.Replace("'", "''");
    }
}
