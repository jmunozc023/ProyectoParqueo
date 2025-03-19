namespace ParqueoApp3.Models
{
    public class Espacio
    {
        public int id_espacio { get; set; }
        public string tipo_espacio { get; set; }
        public bool disponibilidad { get; set; }
        public int id_parqueo { get; set; }
        public Parqueo Parqueo { get; set; }
    }
}
