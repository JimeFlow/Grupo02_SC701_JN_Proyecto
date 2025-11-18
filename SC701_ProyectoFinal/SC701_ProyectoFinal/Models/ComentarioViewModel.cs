namespace SC701_ProyectoFinal.Models
{
    public class ComentarioViewModel
    {
        public required string NombreUsuario { get; set; }
        public string? Libro { get; set; }
        public required string Comentario { get; set; }
        public int Rating { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
