using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Web.Models;

// Una fila del reporte: un préstamo con su socio y todos sus libros.
public class PrestamoReporte
{
    [Display(Name = "N.°")]
    public int PrestamoId { get; set; }

    [Display(Name = "Socio")]
    public string SocioNombre { get; set; } = string.Empty;

    [Display(Name = "DNI")]
    public string SocioDNI { get; set; } = string.Empty;

    // Títulos unidos con '|' por STRING_AGG en usp_Prestamos_Reporte.
    [Display(Name = "Libros")]
    public string Libros { get; set; } = string.Empty;

    [Display(Name = "Libros pendientes")]
    public int LibrosPendientes { get; set; }

    [Display(Name = "Fecha de préstamo")]
    [DataType(DataType.Date)]
    public DateTime FechaPrestamo { get; set; }

    [Display(Name = "Fecha límite")]
    [DataType(DataType.Date)]
    public DateTime FechaLimite { get; set; }

    [Display(Name = "Estado")]
    public string Estado { get; set; } = string.Empty;

    // Propiedades calculadas: no existen en la base de datos.
    public IEnumerable<string> ListaLibros => Libros.Split('|', StringSplitOptions.RemoveEmptyEntries);

    public bool Vencido => Estado == "Pendiente" && FechaLimite.Date < DateTime.Today;
}
