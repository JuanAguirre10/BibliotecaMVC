using System.Data;
using Dapper;
using Biblioteca.Web.Models;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Web.Repositorios;

public class LibroRepositorio : ILibroRepositorio
{
    private readonly string _connectionString;

    // IConfiguration lee la cadena de conexión desde appsettings.json
    public LibroRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'BibliotecaDB' en appsettings.json.");
    }

    public async Task<IEnumerable<Libro>> ListarAsync()
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Libro>(
            "usp_Libros_Listar",
            commandType: CommandType.StoredProcedure);
    }

    // Busca solo en el título; el autor no se considera.
    public async Task<IEnumerable<Libro>> BuscarPorTituloAsync(string titulo)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryAsync<Libro>(
            "usp_Libros_BuscarPorTitulo",
            new { Titulo = titulo },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Libro?> ObtenerPorIdAsync(int libroId)
    {
        using var connection = new SqlConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<Libro>(
            "usp_Libros_ObtenerPorId",
            new { LibroId = libroId },
            commandType: CommandType.StoredProcedure);
    }

    // AutorNombre y AutorNacionalidad son solo para mostrar: no se envían.
    public async Task InsertarAsync(Libro libro)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "usp_Libros_Insertar",
            new { libro.Titulo, libro.ISBN, libro.AutorId, libro.Ejemplares },
            commandType: CommandType.StoredProcedure);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "usp_Libros_Actualizar",
            new { libro.LibroId, libro.Titulo, libro.ISBN, libro.AutorId, libro.Ejemplares },
            commandType: CommandType.StoredProcedure);
    }

    // Eliminación lógica: el procedimiento solo marca Activo = 0.
    public async Task EliminarAsync(int libroId)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(
            "usp_Libros_Eliminar",
            new { LibroId = libroId },
            commandType: CommandType.StoredProcedure);
    }

    // Al crear se envía libroId = 0; al editar, el id del propio libro para no contarlo.
    public async Task<bool> ExisteIsbnAsync(string isbn, int libroId)
    {
        using var connection = new SqlConnection(_connectionString);

        int count = await connection.ExecuteScalarAsync<int>(
            "usp_Libros_ExisteIsbn",
            new { ISBN = isbn, LibroId = libroId },
            commandType: CommandType.StoredProcedure);

        return count > 0;
    }
}
