using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using CompanyManagement.Api.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CompanyManagement.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = Roles.Admin)]
public class ReportesController : ControllerBase
{
    private readonly ReportSettings _settings;

    public ReportesController(IOptions<ReportSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("clientes-activos")]
    [ProducesResponseType(typeof(ReporteDto), StatusCodes.Status200OK)]
    public IActionResult ClientesActivos([FromQuery] bool redirigir = false) =>
        Responder(_settings.ClientesActivos, "Clientes con al menos una orden y su total de órdenes", redirigir);

    [HttpGet("ingresos-clientes")]
    [ProducesResponseType(typeof(ReporteDto), StatusCodes.Status200OK)]
    public IActionResult IngresosClientes([FromQuery] bool redirigir = false) =>
        Responder(_settings.IngresosClientes, "Total de ingresos generados por cada cliente", redirigir);

    [HttpGet("clientes-inactivos")]
    [ProducesResponseType(typeof(ReporteDto), StatusCodes.Status200OK)]
    public IActionResult ClientesInactivos([FromQuery] bool redirigir = false) =>
        Responder(_settings.ClientesInactivos, "Clientes registrados en el último mes sin órdenes", redirigir);

    private IActionResult Responder(string reporte, string descripcion, bool redirigir)
    {
        var dto = ConstruirReporte(reporte, descripcion);
        return redirigir ? Redirect(dto.Url) : Ok(dto);
    }

    private ReporteDto ConstruirReporte(string reporte, string descripcion)
    {
        var servidor = _settings.ReportServerUrl.TrimEnd('/');
        var carpeta = "/" + _settings.ReportsFolder.Trim('/');
        var ruta = Uri.EscapeDataString($"{carpeta}/{reporte}").Replace("%2F", "/");
        var baseUrl = $"{servidor}?{ruta}&rs:Command=Render";

        return new ReporteDto(
            reporte,
            descripcion,
            baseUrl,
            $"{baseUrl}&rs:Format=PDF",
            $"{baseUrl}&rs:Format=EXCELOPENXML");
    }
}
