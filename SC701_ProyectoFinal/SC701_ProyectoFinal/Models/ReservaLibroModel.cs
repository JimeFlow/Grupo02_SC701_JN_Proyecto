namespace SC701_ProyectoFinal.Models
{
    public class ReservaLibroModel
    {
        public int Id_Libro { get; set; }
        public int Id_Usuario { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
