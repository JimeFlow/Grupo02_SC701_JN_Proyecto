namespace ProyectoAPI.Models
{
    public class ResponderContactoRequest
    {
        public int Id_Contacto { get; set; }
        public string Respuesta { get; set; } = string.Empty;
    }
}
