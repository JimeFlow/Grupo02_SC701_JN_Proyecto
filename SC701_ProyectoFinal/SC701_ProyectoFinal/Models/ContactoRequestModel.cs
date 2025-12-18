using System.ComponentModel.DataAnnotations;

namespace SC701_ProyectoFinal.Models
{
    public class ContactoRequestModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        public string Mensaje { get; set; } = string.Empty;
    }
}
