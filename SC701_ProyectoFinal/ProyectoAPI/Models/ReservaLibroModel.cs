namespace ProyectoAPI.Models
{
    public class ReservaLibroModel
    {
        public int Id_Libro { get; set; }
        public int Id_Usuario { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Titulo { get; set; } = string.Empty;

        public int Id_Movimiento { get; set; }

        public string Imagen_URL { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
    }
}
