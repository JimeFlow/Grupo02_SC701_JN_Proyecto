namespace ProyectoAPI.Models
{
    public class ReservaRequestModel
    {
        public int Id_Libro { get; set; }
        public int Id_Usuario { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
    }
}

