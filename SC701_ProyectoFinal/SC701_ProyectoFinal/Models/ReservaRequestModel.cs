namespace SC701_ProyectoFinal.Models
{
    public class ReservaRequestModel
    {
        public int Id_Usuario { get; set; }
        public int Id_Ejemplar { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;


    }
}
