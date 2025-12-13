namespace SC701_ProyectoFinal.Models
{
    public class LibroRequestModel
    {
        public string ISBN { get; set; }
        
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public int Anio { get; set; }
        public string Imagen_URL { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Id_Categoria { get; set; }
    }
}
