namespace SC701_ProyectoFinal.Models
{
    public class ComentarioModel
    {
        public int Id_Usuario { get; set; }
        public int Id_Libro { get; set; }
        public string Comentario { get; set; }  
        public int Rating { get; set; }
    }
}
