namespace ProyectoAPI.Models
{
    public class ReservaRecordatorioCorreoModel
    {
        public int Id_Movimiento { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
        public int Id_Usuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
}
