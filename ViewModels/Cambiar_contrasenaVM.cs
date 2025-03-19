using System.ComponentModel.DataAnnotations;

namespace ParqueoApp3.ViewModels
{
    public class Cambiar_contrasenaVM
    {
        public string Correo { get; set; }
        [Required(ErrorMessage = "El campo es obligatorio")]
        public string ContrasenaActual { get; set; }
        [Required(ErrorMessage = "El campo es obligatorio")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres.")]
        public string NuevaContrasena { get; set; }
        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [Compare("NuevaContrasena", ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
        public string ConfirmarContrasena { get; set; }
    }
}
