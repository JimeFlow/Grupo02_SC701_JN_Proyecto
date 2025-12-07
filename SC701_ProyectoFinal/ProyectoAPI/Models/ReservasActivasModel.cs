namespace ProyectoAPI.Models
{
    public class ReservasActivasModel
    {
        public int Id { get; set; }
        public required string Tipo { get; set; }
        public DateTime Fecha { get; set; }

        public DateTime FechaVencimiento { get; set; }
        public int IdEstado { get; set; }

        public string? Nombre { get; set; }
        public string? Titulo { get; set; }
    }
}
