using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class SeguridadRequestModel
    {
        [Required]
        public int Id_Usuario { get; set; }
        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }
}
