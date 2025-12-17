using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class EjemplarRequestModel
    {
        [Required]
        public string CodigoEjemplar { get; set; }
        public int Id_Libro { get; set; }
        public string Estado { get; set; }
        public string? Ubicacion { get; set; }

        public int Cantidad { get; set; }
    }
}
