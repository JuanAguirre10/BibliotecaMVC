using System.Globalization;
using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Web.Controllers;

public class PrestamosController : Controller
{
    private readonly IPrestamoRepositorio _prestamoRepositorio;

    public PrestamosController(IPrestamoRepositorio prestamoRepositorio)
    {
        _prestamoRepositorio = prestamoRepositorio;
    }

    // GET: /Prestamos/Reporte?desde=2026-08-01&hasta=2026-10-07
    // Las fechas llegan por GET (query string), así el reporte se puede recargar o compartir.
    public async Task<IActionResult> Reporte(DateTime? desde, DateTime? hasta)
    {
        // Sin fechas: los últimos 90 días (coincide con el rango rápido de la vista).
        var hoy = DateTime.Today;
        desde ??= hoy.AddDays(-90);
        hasta ??= hoy;

        // El rango elegido se conserva para volver a llenar los campos del filtro.
        ViewData["Desde"] = desde.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        ViewData["Hasta"] = hasta.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (desde > hasta)
        {
            ModelState.AddModelError(string.Empty, "La fecha desde no puede ser mayor que la fecha hasta.");
            return View(Enumerable.Empty<PrestamoReporte>());
        }

        var prestamos = await _prestamoRepositorio.ReporteAsync(desde.Value, hasta.Value);
        return View(prestamos);
    }
}
