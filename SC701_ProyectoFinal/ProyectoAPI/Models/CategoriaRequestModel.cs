using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class CategoriaRequestModel
    {
        [Required]
        public string Tipo { get; set; } = string.Empty;
    }
}
