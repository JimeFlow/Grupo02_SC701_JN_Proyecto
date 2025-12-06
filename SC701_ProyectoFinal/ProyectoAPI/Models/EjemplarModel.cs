namespace ProyectoAPI.Models
{
    public class EjemplarModel
    {
        public int Id_Ejemplar { get; set; }
        public string CodigoEjemplar { get; set; }
        public string Estado { get; set; }
        public string Ubicacion { get; set; }
        public DateTime Fecha_Registro { get; set; }

        public int Id_Libro { get; set; }
        public string Titulo { get; set; }  
    }
}

