using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class ReservaAdminRequestModel
    {
        [Required]
        public int Id_Ejemplar { get; set; }
        [Required]
        public int Id_Usuario { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public DateTime Fecha_Vencimiento { get; set; }
    }
}
