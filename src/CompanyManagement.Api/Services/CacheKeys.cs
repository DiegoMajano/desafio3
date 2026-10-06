namespace CompanyManagement.Api.Services;

public static class CacheKeys
{
    public const string Clientes = "clientes:todos";
    public const string Ordenes = "ordenes:todas";

    public static string Cliente(int id) => $"clientes:{id}";
}
