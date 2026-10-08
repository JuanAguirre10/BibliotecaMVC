using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

public class SociosController : Controller
{
    private readonly ISocioRepositorio _socioRepositorio;

    public SociosController(ISocioRepositorio socioRepositorio)
    {
        _socioRepositorio = socioRepositorio;
    }

    // GET: /Socios
    public async Task<IActionResult> Index()
    {
        var socios = await _socioRepositorio.ListarAsync();
        return View(socios);
    }

    // GET: /Socios/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Socios/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Socio socio)
    {
        // Si el DNI ya existe se muestra el error en el formulario,
        // en lugar de dejar que el UNIQUE de la base haga fallar la página.
        if (!string.IsNullOrWhiteSpace(socio.DNI) && await _socioRepositorio.ExisteDniAsync(socio.DNI))
        {
            ModelState.AddModelError(nameof(Socio.DNI), "Ya existe un socio registrado con ese DNI.");
        }

        if (!ModelState.IsValid)
        {
            return View(socio);
        }

        await _socioRepositorio.InsertarAsync(socio);

        TempData["Mensaje"] = "Socio registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
