using ParqueoApp3.ViewModels;
namespace ParqueoApp3.ViewModels
{
    public class EspaciosVM
    {
        public string tipo_espacio { get; set; } // Motocicleta, Automóvil, Discapacitados
        public int cantidad { get; set; } // Número total de este tipo de espacio
        public bool disponibilidad { get; set; }
    }
}
