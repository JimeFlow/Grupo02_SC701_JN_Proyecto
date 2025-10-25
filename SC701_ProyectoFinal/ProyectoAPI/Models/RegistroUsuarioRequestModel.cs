using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class RegistroUsuarioRequestModel
    {
        [Required]
        public string Nombre { get; set; } = String.Empty;
        [Required]
        public string Apellidos { get; set; } = String.Empty;

        [Required]
        public string Identificacion { get; set; } = String.Empty;

        [Required]
        public string Correo { get; set; } = String.Empty;

        [Required]
        public string Contrasena { get; set; } = String.Empty;

        [Required]
        public string Telefono { get; set; } = String.Empty;
    }
}
