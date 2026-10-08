using Biblioteca.Web.Models;

namespace Biblioteca.Web.Repositorios;

public interface IAutorRepositorio
{
    Task<IEnumerable<Autor>> ListarAsync();
}
