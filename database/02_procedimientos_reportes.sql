/*
  Procedimientos que alimentan los reportes de SSRS.
  También los crea la migración ReportesProcedimientos.
*/
USE CompanyManagement;
GO

-- Reporte 1: clientes con al menos una orden
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
END
GO

-- Reporte 2: ingresos totales por cliente
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
END
GO

-- Reporte 3: clientes registrados en el último mes que todavía no tienen órdenes
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
END
GO
