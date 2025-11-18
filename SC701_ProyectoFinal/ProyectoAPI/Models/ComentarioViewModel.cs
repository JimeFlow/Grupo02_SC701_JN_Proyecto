namespace ProyectoAPI.Models
{
    public class ComentarioViewModel : ComentarioModel
    {
        public required string NombreUsuario { get; set; }
        public required string Libro { get; set; }
        public required string Comentario { get; set; }
        public new int Rating { get; set; }
        public new DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
