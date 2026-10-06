using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CompanyManagement.Tests.Controllers;

public class ReportesControllerTests
{
    private readonly ReportesController _controller = new(Options.Create(new ReportSettings
    {
        ReportServerUrl = "http://servidor/ReportServer",
        ReportsFolder = "/CompanyManagement",
        ClientesActivos = "ClientesActivos",
        IngresosClientes = "IngresosPorCliente",
        ClientesInactivos = "ClientesInactivos"
    }));

    [Fact]
    public void ClientesActivos_DevuelveUrlDelReporteEnSsrs()
    {
        // Act
        var resultado = _controller.ClientesActivos();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var dto = Assert.IsType<ReporteDto>(ok.Value);
        Assert.Equal("http://servidor/ReportServer?/CompanyManagement/ClientesActivos&rs:Command=Render", dto.Url);
        Assert.EndsWith("rs:Format=PDF", dto.UrlPdf);
    }

    [Fact]
    public void IngresosClientes_ConRedirigir_HaceRedirectAlServidorDeReportes()
    {
        // Act
        var resultado = _controller.IngresosClientes(redirigir: true);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(resultado);
        Assert.Contains("/CompanyManagement/IngresosPorCliente", redirect.Url);
    }
}
