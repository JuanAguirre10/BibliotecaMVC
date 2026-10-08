using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Web.Models;

public class Socio
{
    public int SocioId { get; set; }

    [Display(Name = "DNI")]
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI solo puede contener 8 dígitos.")]
    public string DNI { get; set; } = string.Empty;

    [Display(Name = "Nombre completo")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
    [DataType(DataType.Text)]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede tener más de 100 caracteres.")]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    // Viene de usp_Socios_Listar: libros que el socio todavía no devuelve (solo para mostrar).
    [Display(Name = "Libros pendientes")]
    public int LibrosPendientes { get; set; }
}
