using Microsoft.AspNetCore.Mvc;
using ParqueoApp3.Data;
using ParqueoApp3.Models;
using Microsoft.EntityFrameworkCore;
using ParqueoApp3.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNetCoreGeneratedDocument;

namespace ParqueoApp3.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDBcontext;
        public AccesoController(AppDBContext appDBContext)
        {
            _appDBcontext = appDBContext;
        }
        [HttpGet]
        public IActionResult AdministrarUsuarios()
        {
            ViewData["Usuarios"] = _appDBcontext.Usuarios.ToList();
            ViewData["Vehiculos"] = _appDBcontext.Vehiculos.ToList();
            return View();
        }
        // ...

        [HttpPost]
        public async Task<IActionResult> AdministrarUsuarios(AdministrarUsuariosVM modelo)
        {
            // Check if the email already exists
            bool emailExists = await _appDBcontext.Usuarios.AnyAsync(u => u.correo == modelo.correo);
            if (emailExists)
            {
                ViewData["Mensaje"] = "El correo ya está registrado";
                return View();
            }

            // Create new user
            Usuario usuario = new Usuario
            {
                nombre = modelo.nombre,
                apellido = modelo.apellido,
                correo = modelo.correo,
                password = modelo.password,
                role = modelo.role
            };

            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _appDBcontext.Usuarios.AddAsync(usuario);
                    await _appDBcontext.SaveChangesAsync();

                    var vehiculos = new List<Vehiculo>();

                    if (!string.IsNullOrEmpty(modelo.placa1) && !string.IsNullOrEmpty(modelo.tipo_vehiculo1))
                    {
                        vehiculos.Add(new Vehiculo
                        {
                            placa = modelo.placa1,
                            tipo_vehiculo = modelo.tipo_vehiculo1,
                            id_usuario = usuario.id_usuario
                        });
                    }

                    if (!string.IsNullOrEmpty(modelo.placa2) && !string.IsNullOrEmpty(modelo.tipo_vehiculo2))
                    {
                        vehiculos.Add(new Vehiculo
                        {
                            placa = modelo.placa2,
                            tipo_vehiculo = modelo.tipo_vehiculo2,
                            id_usuario = usuario.id_usuario
                        });
                    }

                    if (vehiculos.Count > 0)
                    {
                        await _appDBcontext.Vehiculos.AddRangeAsync(vehiculos);
                        await _appDBcontext.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    ViewData["Mensaje"] = "Usuario y vehículos registrados exitosamente";
                    return RedirectToAction("AdministrarUsuarios");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ViewData["Mensaje"] = "Ocurrió un error al registrar el usuario y los vehículos.";
                    return View();
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> ModificarUsuarios(string correo)
        {
            var usuario = await _appDBcontext.Usuarios.Where(u => u.correo == correo).FirstOrDefaultAsync();
            if (usuario == null) return NotFound();

            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync();

            var modelo = new AdministrarUsuariosVM
            {
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                correo = usuario.correo,
                password = usuario.password,
                role = usuario.role,
                placa1 = vehiculos.Count > 0 ? vehiculos[0].placa : null,
                tipo_vehiculo1 = vehiculos.Count > 0 ? vehiculos[0].tipo_vehiculo : null,
                placa2 = vehiculos.Count > 1 ? vehiculos[1].placa : null,
                tipo_vehiculo2 = vehiculos.Count > 1 ? vehiculos[1].tipo_vehiculo : null
            };

            return View("ModificarUsuarios", modelo);
        }
        [HttpPost]
        public async Task<IActionResult> ModificarUsuarios(AdministrarUsuariosVM modelo, string _method)
        {
            if(_method == "PUT")
            {
                var usuario = await _appDBcontext.Usuarios.Where(u => u.correo == modelo.correo).FirstOrDefaultAsync();
                if (usuario == null) return NotFound();

                usuario.nombre = modelo.nombre;
                usuario.apellido = modelo.apellido;
                usuario.correo = modelo.correo;
                usuario.password = modelo.password;
                usuario.role = modelo.role;
                _appDBcontext.Usuarios.Update(usuario);

                var id_usuario = await _appDBcontext.Usuarios.Where(u => u.correo == modelo.correo).Select(u => u.id_usuario).FirstOrDefaultAsync();
                var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == id_usuario).ToListAsync();
                if (vehiculos.Count > 0)
                {
                    if (!string.IsNullOrEmpty(modelo.placa1) && !string.IsNullOrEmpty(modelo.tipo_vehiculo1))
                    {
                        vehiculos[0].placa = modelo.placa1;
                        vehiculos[0].tipo_vehiculo = modelo.tipo_vehiculo1;
                        _appDBcontext.Vehiculos.Update(vehiculos[0]);
                    }
                }
                if (vehiculos.Count > 1)
                {
                    if (!string.IsNullOrEmpty(modelo.placa2) && !string.IsNullOrEmpty(modelo.tipo_vehiculo2))
                    {
                        vehiculos[1].placa = modelo.placa2;
                        vehiculos[1].tipo_vehiculo = modelo.tipo_vehiculo2;
                        _appDBcontext.Vehiculos.Update(vehiculos[1]);
                    }
                }
                await _appDBcontext.SaveChangesAsync();
                return RedirectToAction("AdministrarUsuarios");
            }
            return BadRequest();
        }

       

        [HttpDelete]
        public async Task<IActionResult> AdministrarUsuarios(int id_usuario)
        {
            var usuario = await _appDBcontext.Usuarios.FindAsync(id_usuario);
            if (usuario == null) return NotFound();

            // Find and remove associated vehicles
            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync();
            if (vehiculos.Any())
            {
                _appDBcontext.Vehiculos.RemoveRange(vehiculos);
            }

            _appDBcontext.Usuarios.Remove(usuario);
            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("AdministrarUsuarios");
        }
        // Esta presentando errores----------------------------------------------

        [HttpGet]
        public async Task<IActionResult> PerfilUsuario()
        {
            // Obtener el correo del usuario autenticado desde las claims
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userEmail == null) return Unauthorized();
            var usuario = await _appDBcontext.Usuarios.FirstOrDefaultAsync(u => u.correo == userEmail);
            if (usuario == null) return NotFound();
            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync();

            var modelo = new AdministrarUsuariosVM
            {
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                correo = usuario.correo,
                password = usuario.password,
                placa1 = vehiculos.Count > 0 ? vehiculos[0].placa : null,
                tipo_vehiculo1 = vehiculos.Count > 0 ? vehiculos[0].tipo_vehiculo : null,
                placa2 = vehiculos.Count > 1 ? vehiculos[1].placa : null,
                tipo_vehiculo2 = vehiculos.Count > 1 ? vehiculos[1].tipo_vehiculo : null
            };

            return View(modelo);
        }

        // Crear metodos para el perfil de usuario registrado en el sistema, el cual permita modificar los datos del usuario y los vehiculos asociados a este

        [HttpPost]
        public async Task<IActionResult> PerfilUsuario(PerfilVM modelo)
        {
            // Obtener el correo del usuario autenticado desde las claims
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
            if (userEmail == null) return Unauthorized();
            var usuario = await _appDBcontext.Usuarios.FindAsync(userEmail);
            if (usuario == null) return NotFound();
            usuario.nombre = modelo.nombre;
            usuario.apellido = modelo.apellido;
            usuario.correo = modelo.correo;
            usuario.password = modelo.password;
            _appDBcontext.Usuarios.Update(usuario);
            // Update the vehicles associated with the user
            //var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync();
            //foreach (var vehiculo in vehiculos)
            //{
            //    placa =vehiculos.FirstOrDefault()?.placa // Assuming modelo has a property for the new plate number
            //    _appDBcontext.Vehiculos.Update(vehiculo);
            //}
            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }


        //--------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult LogIn()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInVM modelo)
        {
            Usuario? usuario_encontrado = await _appDBcontext.Usuarios
                .Where(u => u.correo == modelo.correo && u.password == modelo.password)
                .FirstOrDefaultAsync();
            TempData["UserEmail"] = usuario_encontrado.correo;
            TempData["UserRole"] = usuario_encontrado.role;
            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }

            if (usuario_encontrado.password == "default_password") // Assuming "default_password" is the initial password
            {
                ViewData["Mensaje"] = "Debe cambiar su contraseña la primera vez que ingresa";
                return RedirectToAction("CambiarContrasena", "Acceso");
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
            if (usuario_encontrado.role == "Seguridad")
            {
                ViewData["Mensaje"] = "Debe seleccionar el parqueo al que está asignado";
                return RedirectToAction("SeleccionarParqueo", "Acceso");
            }
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult CambiarContrasena(string correo)
        {
            ViewData["Correo"] = correo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContrasena(string correo, string nuevaContrasena)
        {
            Usuario? usuario = await _appDBcontext.Usuarios.Where(u => u.correo == correo).FirstOrDefaultAsync();
            if (usuario == null) return NotFound();

            usuario.password = nuevaContrasena;
            _appDBcontext.Usuarios.Update(usuario);
            await _appDBcontext.SaveChangesAsync();

            return RedirectToAction("LogIn", "Acceso");
        }

        [HttpGet]
        public IActionResult SeleccionarParqueo()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            var parqueos = _appDBcontext.Parqueos.ToList();
            ViewData["Parqueos"] = parqueos;
            ViewData["UserEmail"] = TempData["UserEmail"];
            ViewData["UserRole"] = TempData["UserRole"];

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeleccionarParqueo(string nombre_parqueo, string UserEmail, string UserRole)
        {
            if (nombre_parqueo == null) return NotFound();

            if (UserEmail == null || UserRole == null) return Unauthorized();

            // Add the assigned parqueo to the user's claims
            List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, UserEmail),
                    new Claim(ClaimTypes.Role, UserRole),
                    new Claim(ClaimTypes.StreetAddress, nombre_parqueo)
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
        public async Task<IActionResult> Parqueos()
        {
            var parqueos = await _appDBcontext.Parqueos.ToListAsync();
            var espacios = await _appDBcontext.Espacios.ToListAsync();

            var viewModel = new ParqueosViewModel
            {
                Parqueos = parqueos,
                Espacios = espacios
            };

            return View(viewModel);
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
    public class ParqueosViewModel
    {
        public List<Parqueo> Parqueos { get; set; }
        public List<Espacio> Espacios { get; set; }
    }
}
