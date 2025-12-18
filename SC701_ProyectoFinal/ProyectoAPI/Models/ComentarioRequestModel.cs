namespace ProyectoAPI.Models
{
    public class ComentarioRequestModel
    {
        public int Id_Ejemplar { get; set; }
        public int Id_Usuario { get; set; }
        public string Comentario { get; set; } = string.Empty;
        public int Rating { get; set; }
    }
}
