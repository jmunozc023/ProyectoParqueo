namespace ParqueoApp3.Models
{
    public class Usuario
    {
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string correo { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public bool RequiereCambioContrasena { get; set; }
    }
}
