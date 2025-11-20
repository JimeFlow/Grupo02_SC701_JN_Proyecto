namespace ProyectoAPI.Models
{
    public class ReservaLibroModel
    {
        public int LibroId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int EstadoReserva { get; set; } 
    }
}
