namespace SC701_ProyectoFinal.Models
{
    public class LibroViewModel
    {
        public int Id_Libro { get; set; }
        public int Id_Ejemplar { get; set; }
        public required string Titulo { get; set; }
        public required string Autor { get; set; }
        public required string Descripcion { get; set; }
        public int AnioPublicacion { get; set; }
        public required string ISBN { get; set; }
        public required string Imagen_URL { get; set; }
        public int EstadoId { get; set; }
        public List<ComentarioViewModel> Comentarios { get; set; } = new List<ComentarioViewModel>();
    }
}
