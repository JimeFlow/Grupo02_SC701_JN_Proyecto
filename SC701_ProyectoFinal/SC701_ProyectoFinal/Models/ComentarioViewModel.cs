namespace SC701_ProyectoFinal.Models
{
    public class ComentarioViewModel
    {
        // Para crear el comentario
        public int Id_Ejemplar { get; set; }

        public string Comentario { get; set; } = string.Empty;
        public int Rating { get; set; }

        // Para mostrar comentarios
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

    }
}
