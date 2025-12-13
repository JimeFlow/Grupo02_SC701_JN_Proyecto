namespace SC701_ProyectoFinal.Models
{
    public class ReservaViewModel
    {
        public int Id_Movimiento { get; set; }

        public int Id_Libro { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Imagen_URL { get; set; } = string.Empty;

        public DateTime FechaReserva { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public string Estado { get; set; } = string.Empty;

    }
}
