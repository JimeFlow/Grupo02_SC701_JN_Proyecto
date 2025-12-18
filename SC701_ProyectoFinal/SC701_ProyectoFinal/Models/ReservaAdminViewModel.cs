namespace SC701_ProyectoFinal.Models
{
    public class ReservaAdminViewModel
    {
        public int Id_Ejemplar { get; set; }
        public int Id_Usuario { get; set; }

        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
    }
}
