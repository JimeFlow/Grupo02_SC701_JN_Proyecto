namespace SC701_ProyectoFinal.Models
{
    public class UsuariosSancion
    {
        public int Id_Sancion { get; set; }
        public int Id_Usuario { get; set; }

        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Finalizacion { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
    }
}
