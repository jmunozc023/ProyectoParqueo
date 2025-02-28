namespace ParqueoApp3.Models
{
    public class Asig_vehiculo
    {
        public int id_asig_vehiculo { get; set; }
        public int id_vehiculo { get; set; }
        public int id_espacio { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public DateTime fecha_salida { get; set; }
    }
}
