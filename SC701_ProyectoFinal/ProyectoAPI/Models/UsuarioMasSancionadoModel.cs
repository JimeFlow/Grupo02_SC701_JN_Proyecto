namespace ProyectoAPI.Models
{
    public class UsuarioMasSancionadoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Total { get; set;}
        public DateTime FinProxFecha { get; set; }
    }
}
