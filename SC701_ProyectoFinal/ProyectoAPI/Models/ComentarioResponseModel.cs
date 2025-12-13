namespace ProyectoAPI.Models
{
    public class ComentarioResponseModel
    {
        public int Id_Comentario { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime Creacion { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
    }
}
