using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Biblioteca.Web.Controllers;

public class LibrosController : Controller
{
    private readonly ILibroRepositorio _libroRepositorio;
    private readonly IAutorRepositorio _autorRepositorio;

    // Los repositorios llegan por inyección de dependencias (se registraron en Program.cs).
    public LibrosController(ILibroRepositorio libroRepositorio, IAutorRepositorio autorRepositorio)
    {
        _libroRepositorio = libroRepositorio;
        _autorRepositorio = autorRepositorio;
    }

    // GET: /Libros                  -> todos los libros activos
    // GET: /Libros?titulo=ciudad    -> solo los que contienen ese texto en el título
    public async Task<IActionResult> Index(string? titulo)
    {
        titulo = string.IsNullOrWhiteSpace(titulo) ? null : titulo.Trim();

        var libros = titulo == null
            ? await _libroRepositorio.ListarAsync()
            : await _libroRepositorio.BuscarPorTituloAsync(titulo);

        // Se conserva el texto buscado para volver a mostrarlo en la caja de búsqueda.
        ViewData["Busqueda"] = titulo;

        // La búsqueda en vivo (fetch desde site.js) solo necesita las filas de la tabla.
        // Vary evita que el navegador confunda en caché la página completa con solo las filas.
        Response.Headers.Vary = "X-Requested-With";
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return PartialView("_LibrosResultados", libros);
        }

        return View(libros);
    }

    // GET: /Libros/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);

        if (libro == null)
        {
            return NotFound();
        }

        return View(libro);
    }

    // GET: /Libros/Create
    public async Task<IActionResult> Create()
    {
        await CargarAutoresAsync();
        return View();
    }

    // POST: /Libros/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        // Validación propia: el ISBN no se puede repetir (al crear se envía LibroId = 0).
        if (!string.IsNullOrWhiteSpace(libro.ISBN) && await _libroRepositorio.ExisteIsbnAsync(libro.ISBN, 0))
        {
            ModelState.AddModelError(nameof(Libro.ISBN), "Ya existe un libro registrado con ese ISBN.");
        }

        // Si alguna validación falló, se vuelve a mostrar el formulario con los errores.
        if (!ModelState.IsValid)
        {
            await CargarAutoresAsync(libro.AutorId);
            return View(libro);
        }

        await _libroRepositorio.InsertarAsync(libro);

        TempData["Mensaje"] = "Libro registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Libros/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);

        if (libro == null)
        {
            return NotFound();
        }

        await CargarAutoresAsync(libro.AutorId);
        return View(libro);
    }

    // POST: /Libros/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        // El id de la ruta y el del formulario deben coincidir.
        if (id != libro.LibroId)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(libro.ISBN) && await _libroRepositorio.ExisteIsbnAsync(libro.ISBN, libro.LibroId))
        {
            ModelState.AddModelError(nameof(Libro.ISBN), "Ya existe otro libro registrado con ese ISBN.");
        }

        if (!ModelState.IsValid)
        {
            await CargarAutoresAsync(libro.AutorId);
            return View(libro);
        }

        await _libroRepositorio.ActualizarAsync(libro);

        TempData["Mensaje"] = "Libro actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Libros/Delete/5  -> pide confirmación
    public async Task<IActionResult> Delete(int id)
    {
        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);

        if (libro == null)
        {
            return NotFound();
        }

        return View(libro);
    }

    // POST: /Libros/Delete/5  -> eliminación lógica (Activo = 0)
    // Se llama DeleteConfirmado porque no puede tener la misma firma que el GET.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmado(int id)
    {
        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);

        if (libro == null)
        {
            return NotFound();
        }

        await _libroRepositorio.EliminarAsync(id);

        TempData["Mensaje"] = "Libro eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Envía a la vista la lista de autores para llenar la lista desplegable (select).
    private async Task CargarAutoresAsync(int? autorSeleccionado = null)
    {
        var autores = await _autorRepositorio.ListarAsync();
        ViewData["Autores"] = new SelectList(autores, "AutorId", "Nombre", autorSeleccionado);
    }
}
