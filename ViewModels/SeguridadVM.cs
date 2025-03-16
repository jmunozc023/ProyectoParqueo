namespace ParqueoApp3.ViewModels
{
    public class SeguridadVM
    {
        public int id_parqueo { get; set; }
        public string nombre_parqueo { get; set; }
        public string placa { get; set; }
        public DateTime fecha_hora_entrada { get; set; }
        public DateTime fecha_hora_salida { get; set; }
        public bool HayEspaciosDisponibles { get; set; }

    }
}
