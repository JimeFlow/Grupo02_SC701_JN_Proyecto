namespace SC701_ProyectoFinal.Models
{
    public class ReservaViewModel
    {
        public int Id_Libro { get; set; }
        public string? Titulo { get; set; }
        public string? Estado_Libro { get; set; }
        public DateTime FechaInicio { get; set; } = DateTime.Today;
        public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(7);
        public int Id_Usuario { get; set; }

    }
}
