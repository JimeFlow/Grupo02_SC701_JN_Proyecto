namespace ProyectoAPI.Models
{
    public class ComentarioViewModel 
    {
        public int ComentarioId { get; set; }
        public int UsuarioId { get; set; }
        public int LibroId { get; set; }
        public string NombreUsuario { get; set; }
        public string Libro { get; set; }
        public string Comentario { get; set; }
        public int Rating { get; set; }
        public DateTime FechaCreacion { get; set; } 
    }
}
