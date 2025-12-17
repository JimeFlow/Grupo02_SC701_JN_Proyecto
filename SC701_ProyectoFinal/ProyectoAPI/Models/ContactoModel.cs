namespace ProyectoAPI.Models
{
    public class ContactoModel
    {
        public int Id_Contacto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Estado { get; set; } 
    }
}
