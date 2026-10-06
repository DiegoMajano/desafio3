/*
  Datos de ejemplo para probar la API y los reportes.
  Las fechas se calculan a partir de hoy para que el reporte de
  clientes inactivos siempre tenga resultados.
*/
USE CompanyManagement;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
BEGIN
    DECLARE @hoy datetime2 = GETUTCDATE();

    INSERT INTO dbo.Clientes (Nombre, Email, FechaRegistro) VALUES
        (N'Distribuidora La Ceiba',      N'compras@laceiba.com.sv',      DATEADD(DAY, -120, @hoy)),
        (N'Ferretería El Martillo',      N'ventas@elmartillo.com.sv',    DATEADD(DAY, -95,  @hoy)),
        (N'Panadería San Antonio',       N'pedidos@sanantonio.com.sv',   DATEADD(DAY, -80,  @hoy)),
        (N'Farmacia Santa Lucía',        N'admin@farmaciasl.com.sv',     DATEADD(DAY, -60,  @hoy)),
        (N'Café Los Volcanes',           N'info@cafelosvolcanes.com',    DATEADD(DAY, -45,  @hoy)),
        (N'Librería Cuscatlán',          N'contacto@libcuscatlan.com',   DATEADD(DAY, -40,  @hoy)),
        (N'Taller Mecánico Hermanos Paz',N'taller@hermanospaz.com',      DATEADD(DAY, -20,  @hoy)),
        (N'Agroservicio El Rancho',      N'agro@elrancho.com.sv',        DATEADD(DAY, -12,  @hoy)),
        (N'Óptica Visión Clara',         N'citas@visionclara.com.sv',    DATEADD(DAY, -7,   @hoy)),
        (N'Pupusería Doña Tere',         N'donatere@gmail.com',          DATEADD(DAY, -3,   @hoy)),
        (N'Consultora Bosco & Asociados',N'info@boscoasociados.com',     DATEADD(DAY, -1,   @hoy));

    INSERT INTO dbo.Ordenes (ClienteId, FechaOrden, MontoTotal)
    SELECT c.Id, DATEADD(DAY, v.Dias, @hoy), v.Monto
    FROM (VALUES
        (N'compras@laceiba.com.sv',    -110, 1250.00),
        (N'compras@laceiba.com.sv',    -70,   830.50),
        (N'compras@laceiba.com.sv',    -15,  2140.75),
        (N'ventas@elmartillo.com.sv',  -90,   455.20),
        (N'ventas@elmartillo.com.sv',  -30,   610.00),
        (N'pedidos@sanantonio.com.sv', -75,   189.99),
        (N'admin@farmaciasl.com.sv',   -55,  3200.00),
        (N'admin@farmaciasl.com.sv',   -25,  1475.40),
        (N'admin@farmaciasl.com.sv',   -5,    980.00),
        (N'admin@farmaciasl.com.sv',   -2,    215.60),
        (N'info@cafelosvolcanes.com',  -35,   540.00),
        (N'taller@hermanospaz.com',    -10,   725.35)
    ) AS v(Email, Dias, Monto)
    INNER JOIN dbo.Clientes c ON c.Email = v.Email;
END
GO
