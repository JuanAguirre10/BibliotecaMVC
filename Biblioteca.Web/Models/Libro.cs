using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Web.Models;

public class Libro
{
    public int LibroId { get; set; }

    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede tener más de 150 caracteres.")]
    [DataType(DataType.Text)]
    public string Titulo { get; set; } = string.Empty;

    // En la base el ISBN se guarda sin guiones (VARCHAR(13)).
    [Display(Name = "ISBN")]
    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "El ISBN debe tener entre 10 y 13 caracteres.")]
    [RegularExpression(@"^(\d{13}|\d{9}[\dX])$", ErrorMessage = "El ISBN debe tener 13 dígitos (o 10 en el formato antiguo), sin guiones.")]
    [DataType(DataType.Text)]
    public string ISBN { get; set; } = string.Empty;

    [Display(Name = "Autor")]
    [Required(ErrorMessage = "Seleccione un autor.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un autor.")]
    public int AutorId { get; set; }

    [Display(Name = "Ejemplares")]
    [Required(ErrorMessage = "Indique la cantidad de ejemplares.")]
    [Range(0, 999, ErrorMessage = "Los ejemplares deben estar entre 0 y 999.")]
    public int Ejemplares { get; set; }

    // Vienen del INNER JOIN con Autores (solo para mostrar, no se envían al guardar).
    [Display(Name = "Autor")]
    public string? AutorNombre { get; set; }

    [Display(Name = "Nacionalidad")]
    public string? AutorNacionalidad { get; set; }

    // Propiedad calculada: no existe en la base de datos.
    public bool Disponible => Ejemplares > 0;
}
