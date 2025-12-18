namespace SC701_ProyectoFinal.Models
{
    public class ContactoViewModel
    {
        public int Id_Contacto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public string? Respuesta { get; set; }

        public bool Estado { get; set; } 

        public string Asunto { get; set; } = string.Empty;

        public bool Respondido => !Estado;

    }
}
