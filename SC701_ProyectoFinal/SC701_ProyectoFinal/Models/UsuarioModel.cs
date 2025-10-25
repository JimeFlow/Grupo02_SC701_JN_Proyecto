namespace SC701_ProyectoFinal.Models
{
    public class UsuarioModel
    {
        public int Id_Usuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public int Id_Rol { get; set; }
        public string Tipo_Rol { get; set; } = string.Empty;
    }
}
