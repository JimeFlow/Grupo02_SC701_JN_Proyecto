namespace SC701_ProyectoFinal.Models
{
    public class LibroModel
    {
        public int Id_Libro { get; set; }
        public string ISBN { get; set; }
        public string Estado_Libro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Id_Categoria { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public short Anio { get; set; }
        public string Imagen_URL { get; set; }
        public int Id_Estado { get; set; }
        public int Disponibles { get; set; }
    }
}