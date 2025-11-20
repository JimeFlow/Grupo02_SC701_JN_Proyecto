namespace SC701_ProyectoFinal.Models
{
    public class ReservaViewModel
    {
        public int Id { get; set; }

        public int LibroId { get; set; }
        public string? Titulo { get; set; }
        public String? Imagen { get; set; }
        public DateTime FechaReserva { get; set; } 
        public DateTime FechaVencimiento { get; set; } 
        public string? Estado  { get; set; }

    }
}
