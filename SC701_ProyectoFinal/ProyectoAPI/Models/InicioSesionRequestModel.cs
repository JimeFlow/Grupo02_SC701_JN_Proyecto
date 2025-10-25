using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class InicioSesionRequestModel
    {
        [Required]
        public string Correo { get; set; } = String.Empty;

        [Required]
        public string Contrasena { get; set; } = String.Empty;
    }
}
