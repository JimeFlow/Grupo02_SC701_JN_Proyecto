namespace ProyectoAPI.Models
{
    public class FAQModel
    {
        public int Id_FAQ { get; set; }
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
