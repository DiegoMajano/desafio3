using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportesProcedimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_ReporteClientesActivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Nombre,
           c.Email,
           COUNT(o.Id) AS TotalOrdenes
    FROM dbo.Clientes c
    INNER JOIN dbo.Ordenes o ON o.ClienteId = c.Id
    GROUP BY c.Id, c.Nombre, c.Email
    HAVING COUNT(o.Id) >= 1
    ORDER BY TotalOrdenes DESC, c.Nombre;
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_ReporteIngresosPorCliente
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Nombre,
           c.Email,
           CAST(ISNULL(SUM(o.MontoTotal), 0) AS decimal(18,2)) AS MontoTotal
    FROM dbo.Clientes c
    LEFT JOIN dbo.Ordenes o ON o.ClienteId = c.Id
    GROUP BY c.Id, c.Nombre, c.Email
    ORDER BY MontoTotal DESC, c.Nombre;
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.sp_ReporteClientesInactivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Nombre,
           c.Email,
           c.FechaRegistro
    FROM dbo.Clientes c
    WHERE c.FechaRegistro >= DATEADD(MONTH, -1, GETUTCDATE())
      AND NOT EXISTS (SELECT 1 FROM dbo.Ordenes o WHERE o.ClienteId = c.Id)
    ORDER BY c.FechaRegistro DESC;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_ReporteClientesActivos;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_ReporteIngresosPorCliente;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_ReporteClientesInactivos;");
        }
    }
}
