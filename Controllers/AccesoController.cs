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
    }
}
