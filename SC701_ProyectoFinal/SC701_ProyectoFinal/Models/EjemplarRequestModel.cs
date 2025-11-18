namespace SC701_ProyectoFinal.Models
{
    public class EjemplarRequestModel
    {
        public string CodigoEjemplar { get; set; }
        public int Id_Libro { get; set; }
        public string Estado { get; set; }
        public string? Ubicacion { get; set; }
    }
}
