using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class EditarCategoriaRequestModel
    {
        [Required]
        public int Id_Categoria { get; set; }
        [Required]
        public string Tipo { get; set; } = string.Empty;
    }
}
