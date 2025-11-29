namespace ProyectoAPI.Models
{
    public class ComentarioModel
    {
        public int Id_Comentario { get; set; }
        public int Id_Usuario { get; set; }
        public int Id_Libro { get; set; }
        public String Comentario { get; set; }  
        public int Rating { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
