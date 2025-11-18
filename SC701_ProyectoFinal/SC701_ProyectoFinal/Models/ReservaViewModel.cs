namespace SC701_ProyectoFinal.Models
{
    public class ReservaViewModel
    {
        public int LibroId { get; set; }
        public string Libro { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int EstadoReserva { get; set; } // 0: Pendiente, 1: Activa, 2: Cancelada, 3: Completada

    }
}
