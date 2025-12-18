namespace ProyectoAPI.Models
{
    public class FAQRequestModel
    {
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;

        public bool Estado { get; set; } = true;
    }
}
