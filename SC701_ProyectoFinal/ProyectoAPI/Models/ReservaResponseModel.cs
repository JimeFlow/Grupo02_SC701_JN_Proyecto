namespace ProyectoAPI.Models
{
    public class ReservaResponseModel
    {
        public int Id_Movimiento { get; set; }
        public int Id_Libro { get; set; }
        public int Id_Ejemplar { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime Fecha_Vencimiento { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Imagen_URL { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public int Id_Estado { get; set; }
        public int Resultado { get; set; }
    }
}
