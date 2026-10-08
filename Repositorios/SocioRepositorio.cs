using System.Data;
using Dapper;
using Biblioteca.Web.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Web.Repositorios;

public class SocioRepositorio : ISocioRepositorio
{
    private readonly string _connectionString;

    // IConfiguration lee la cadena de conexión desde appsettings.json
    public SocioRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'BibliotecaDB' en appsettings.json.");
    }

    // El procedimiento también devuelve LibrosPendientes de cada socio.
    public async Task<IEnumerable<Socio>> ListarAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Socio>(
            "usp_Socios_Listar",
            commandType: CommandType.StoredProcedure);
    }

    // Si el correo llega vacío, el procedimiento lo guarda como NULL.
    public async Task InsertarAsync(Socio socio)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "usp_Socios_Insertar",
            new { socio.DNI, socio.Nombre, socio.Email },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ExisteDniAsync(string dni)
    {
        using var connection = new SqlConnection(_connectionString);

        int count = await connection.ExecuteScalarAsync<int>(
            "usp_Socios_ExisteDni",
            new { DNI = dni },
            commandType: CommandType.StoredProcedure);

        return count > 0;
    }
}
