namespace SC701_ProyectoFinal.Models
{
    public class ComentarioModel
    {
        public int UsuarioId { get; set; }
        public int LibroId { get; set; }
        public int ComentarioId { get; set; }  
        public int Rating { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
