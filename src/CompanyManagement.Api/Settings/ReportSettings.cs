namespace CompanyManagement.Api.Settings;

public class ReportSettings
{
    public string ReportServerUrl { get; set; } = string.Empty;
    public string ReportsFolder { get; set; } = "/CompanyManagement";
    public string ClientesActivos { get; set; } = "ClientesActivos";
    public string IngresosClientes { get; set; } = "IngresosPorCliente";
    public string ClientesInactivos { get; set; } = "ClientesInactivos";
}
