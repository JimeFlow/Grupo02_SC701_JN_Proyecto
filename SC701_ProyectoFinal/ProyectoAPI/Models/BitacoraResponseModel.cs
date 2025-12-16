namespace ProyectoAPI.Models
{
    public class BitacoraResponseModel
    {
        public DateTime Fecha { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }

}
