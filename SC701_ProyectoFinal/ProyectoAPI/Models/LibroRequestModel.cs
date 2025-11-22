using System.ComponentModel.DataAnnotations;

namespace ProyectoAPI.Models
{
    public class LibroRequestModel
    {
        [Required]
        public string ISBN { get; set; }
        public string Estado_Libro { get; set; }
        public string Descripcion { get; set; } = string.Empty;   
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public short Anio { get; set; }
        public string Imagen_URL { get; set; }
        public int Id_Estado { get; set; }
    }
}
