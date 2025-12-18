namespace SC701_ProyectoFinal.Models
{
    public class BitacoraModel
    {
        public int Id_Log { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreUsuario { get; set; } = string.Empty; // JOIN con Usuario
        public string Modulo_Afectado { get; set; } = string.Empty;
        public string Tipo_Accion { get; set; } = string.Empty;
        public string Descripcion_Accion { get; set; } = string.Empty;
    }

}
