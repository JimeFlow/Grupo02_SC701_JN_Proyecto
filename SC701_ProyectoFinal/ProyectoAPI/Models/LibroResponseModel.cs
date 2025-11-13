namespace ProyectoAPI.Models
{
    public class LibroResponseModel
    {
        public int Id_Libro { get; set; }
        public string ISBN { get; set; }
        public string Estado_Libro { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public short Anio { get; set; }
        public string Imagen_URL { get; set; }
        public string Estado { get; set; } 
    }
}
