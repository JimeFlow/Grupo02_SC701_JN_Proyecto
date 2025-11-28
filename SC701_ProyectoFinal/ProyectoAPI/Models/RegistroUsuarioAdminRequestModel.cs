using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class RegistroUsuarioAdminRequestModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        public string Correo { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;
        [Required]
        public int Id_Rol { get; set; }
    }
}
