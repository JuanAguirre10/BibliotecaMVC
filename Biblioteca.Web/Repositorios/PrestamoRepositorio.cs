using System.Data;
using Dapper;
using Biblioteca.Web.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Web.Repositorios;

public class PrestamoRepositorio : IPrestamoRepositorio
{
    private readonly string _connectionString;

    // IConfiguration lee la cadena de conexión desde appsettings.json
    public PrestamoRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'BibliotecaDB' en appsettings.json.");
    }

    // Una fila por préstamo. .Date quita la hora para que el rango incluya el día "hasta" completo.
    public async Task<IEnumerable<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<PrestamoReporte>(
            "usp_Prestamos_Reporte",
            new { Desde = desde.Date, Hasta = hasta.Date },
            commandType: CommandType.StoredProcedure);
    }
}
