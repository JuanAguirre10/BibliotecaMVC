namespace Biblioteca.Web.Models;

// Solo se usa para llenar la lista desplegable de autores en Libros.
public class Autor
{
    public int AutorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Nacionalidad { get; set; }
}
