namespace SC701_ProyectoFinal.Models
{
    public class ReservaListadoViewModel
    {
        public int Id_Movimiento { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public bool TieneSancionActiva { get; set; }
        public string Correo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
        public int Id_Estado { get; set; }
    }
}
