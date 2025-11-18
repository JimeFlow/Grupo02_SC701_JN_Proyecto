namespace SC701_ProyectoFinal.Models
{
    public class LibroViewModel
    {
        public int LibroId { get; set; }
        public required string Titulo { get; set; }
        public required string Autor { get; set; }
        public required string Descripcion { get; set; }
        public int AnioPublicacion { get; set; }
        public required string ISBN { get; set; }
        public required string Imagen { get; set; }
        public required string Estado { get; set; }
        public int EstadoId { get; set; }
        public List<ComentarioViewModel> Comentarios { get; set; } = new List<ComentarioViewModel>();
    }
}
