namespace SC701_ProyectoFinal.Models
{
    public class FAQViewModel
    {
        public int Id_FAQ { get; set; }
        public string Pregunta { get; set; } = string.Empty;
        public string Respuesta { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
