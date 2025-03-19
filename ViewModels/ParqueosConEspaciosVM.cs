using ParqueoApp3.ViewModels;
namespace ParqueoApp3.ViewModels
{
    public class ParqueosConEspaciosVM
    {
        public int id_parqueo { get; set; }
        public string nombre_parqueo { get; set; }
        public string ubicacion { get; set; }
        public List<EspaciosVM> Espacios { get; set; }
    }
}
