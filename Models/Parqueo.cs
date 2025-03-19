using ParqueoApp3.Models;

namespace ParqueoApp3.Models
{
    public class Parqueo
    {
        public int id_parqueo { get; set; }
        public string nombre_parqueo { get; set; }
        public string ubicacion { get; set; }
        public ICollection<Espacio> Espacios { get; set; }
    }
}
