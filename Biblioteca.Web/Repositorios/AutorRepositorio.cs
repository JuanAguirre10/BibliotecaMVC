using System.Data;
using Dapper;
using Biblioteca.Web.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Web.Repositorios;

public class AutorRepositorio : IAutorRepositorio
{
    private readonly string _connectionString;

    // IConfiguration lee la cadena de conexión desde appsettings.json
    public AutorRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'BibliotecaDB' en appsettings.json.");
    }

    // Solo autores activos; se usa para la lista desplegable de Libros.
    public async Task<IEnumerable<Autor>> ListarAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Autor>(
            "usp_Autores_Listar",
            commandType: CommandType.StoredProcedure);
    }
}
