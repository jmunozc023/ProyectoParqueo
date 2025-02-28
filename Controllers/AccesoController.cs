using Microsoft.AspNetCore.Mvc;
using ParqueoApp3.Data;
using ParqueoApp3.Models;
using Microsoft.EntityFrameworkCore;
using ParqueoApp3.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ParqueoApp3.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public AccesoController(AppDBContext appDBContext) {
            _appDBcontext = appDBContext;
        }
        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            if(modelo.password != modelo.confirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }
            Usuario usuario = new Usuario
            {
                nombre = modelo.nombre,
                apellido = modelo.apellido,
                correo = modelo.correo,
                password = modelo.password,
                role = modelo.role
            };

            await _appDBcontext.Usuarios.AddAsync(usuario);
            await _appDBcontext.SaveChangesAsync();

            if(usuario.id_usuario != 0) return RedirectToAction("LogIn", "Acceso");
            ViewData["Mensaje"] = "Error al registrar el usuario";

            return View();
        }
        [HttpPut]
        
        public async Task<IActionResult> PerfilUsuario(PerfilVM modelo)
        {
            Usuario usuario = await _appDBcontext.Usuarios.FindAsync(modelo.correo);
            if (usuario == null) return NotFound();
            usuario.nombre = modelo.nombre;
            usuario.apellido = modelo.apellido;
            usuario.correo = modelo.correo;
            usuario.password = modelo.password;
            usuario.role = modelo.role;
            _appDBcontext.Usuarios.Update(usuario);

            // Update the vehicles associated with the user
            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync();
            foreach (var vehiculo in vehiculos)
            {
                vehiculo.placa = modelo.placa; // Assuming modelo has a property for the new plate number
                _appDBcontext.Vehiculos.Update(vehiculo);
            }

            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        public IActionResult LogIn()
        {
            if(User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInVM modelo)
        { 
            Usuario? usuario_encontrado = await _appDBcontext.Usuarios.Where(u => u.correo == modelo.correo && u.password == modelo.password).FirstOrDefaultAsync();
            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario_encontrado.correo),
                new Claim(ClaimTypes.Role, usuario_encontrado.role)
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
       
        public async Task<IActionResult> AdminParqueos()
        {
            var parqueos = await _appDBcontext.Parqueos.ToListAsync();
            return View(parqueos);
        }
        [HttpPost]
        public async Task<IActionResult> Parqueos(Parqueo modelo)
        {
            Parqueo parqueo = new Parqueo
            {
                nombre_parqueo = modelo.nombre_parqueo,
                ubicacion = modelo.ubicacion
            };
            await _appDBcontext.Parqueos.AddAsync(modelo);
            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> AdminEspacios()
        {
            var espacios = await _appDBcontext.Espacios.ToListAsync();
            return View(espacios);
        }
        [HttpPost]
        public async Task<IActionResult> Espacios(Espacio modelo)
        {
            Espacio espacio = new Espacio
            {
                tipo_espacio = modelo.tipo_espacio,
                disponibilidad = modelo.disponibilidad
            };
            await _appDBcontext.Espacios.AddAsync(modelo);
            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> ContarEspacios()
        {
            var espaciosContados = await _appDBcontext.Espacios
                .GroupBy(e => new { e.tipo_espacio, e.id_parqueo })
                .Select(g => new
                {
                    TipoEspacio = g.Key.tipo_espacio,
                    IdParqueo = g.Key.id_parqueo,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            return View(espaciosContados);
        }
    }
}
