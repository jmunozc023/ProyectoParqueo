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
        private readonly AppDBContext _appDBcontext; // Asigna el database context a una variable privada
        public AccesoController(AppDBContext appDBContext) // Constructor que recibe el database context
        {
            _appDBcontext = appDBContext;
        }
        [HttpGet]
        public IActionResult AdministrarUsuarios() // Método para mostrar la vista de administración de usuarios
        {
            ViewData["Usuarios"] = _appDBcontext.Usuarios.ToList(); // Obtiene la lista de usuarios y la asigna a la vista
            ViewData["Vehiculos"] = _appDBcontext.Vehiculos.ToList(); // Obtiene la lista de vehículos y la asigna a la vista
            return View(); // Devuelve la vista
        }

        [HttpPost]
        public async Task<IActionResult> AdministrarUsuarios(AdministrarUsuariosVM modelo) // Método para registrar un usuario y sus vehículos
        {
            // Check if the email already exists
            bool emailExists = await _appDBcontext.Usuarios.AnyAsync(u => u.correo == modelo.correo); // Verifica si el correo ya existe
            if (emailExists)
            {
                ViewData["Mensaje"] = "El correo ya está registrado"; // Muestra un mensaje de error en caso de que el correo ya este registrado en la base de datos
                return View();
            }

            // Create new user
            Usuario usuario = new Usuario // Crea un nuevo usuario con los datos ingresados
            {
                nombre = modelo.nombre,
                apellido = modelo.apellido,
                correo = modelo.correo,
                password = modelo.password,
                role = modelo.role
            };

            using (var transaction = await _appDBcontext.Database.BeginTransactionAsync()) // Inicia una transacción para asegurar la integridad de los datos o revertirlos en caso de error
            {
                try
                {
                    await _appDBcontext.Usuarios.AddAsync(usuario);// Agrega el usuario a la base de datos
                    await _appDBcontext.SaveChangesAsync();

                    var vehiculos = new List<Vehiculo>();// Crea una lista de vehículos

                    if (!string.IsNullOrEmpty(modelo.placa1) && !string.IsNullOrEmpty(modelo.tipo_vehiculo1))// Verifica si los campos de placa y tipo de vehículo no están vacíos
                    {
                        vehiculos.Add(new Vehiculo
                        {
                            placa = modelo.placa1,
                            tipo_vehiculo = modelo.tipo_vehiculo1,
                            id_usuario = usuario.id_usuario
                        }); // Agrega el vehículo a la lista
                    }

                    if (!string.IsNullOrEmpty(modelo.placa2) && !string.IsNullOrEmpty(modelo.tipo_vehiculo2)) // Verifica si los campos de placa y tipo de vehículo no están vacíos
                    {
                        vehiculos.Add(new Vehiculo
                        {
                            placa = modelo.placa2,
                            tipo_vehiculo = modelo.tipo_vehiculo2,
                            id_usuario = usuario.id_usuario
                        }); // Agrega el vehículo a la lista
                    }

                    if (vehiculos.Count > 0) // Verifica si hay vehículos en la lista
                    {
                        await _appDBcontext.Vehiculos.AddRangeAsync(vehiculos); // Agrega los vehículos a la base de datos
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
        public async Task<IActionResult> ModificarUsuarios(string correo) // Método para mostrar la vista de modificación de usuarios
        {
            var usuario = await _appDBcontext.Usuarios.Where(u => u.correo == correo).FirstOrDefaultAsync(); // Obtiene el usuario con el correo especificado
            if (usuario == null) return NotFound(); // Si no se encuentra el usuario, devuelve 404

            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync(); // Obtiene los vehículos asociados al usuario

            var modelo = new AdministrarUsuariosVM // Crea un modelo con los datos del usuario y sus vehículos
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

            return View("ModificarUsuarios", modelo); // Devuelve la vista con el modelo
        }
        [HttpPost]
        public async Task<IActionResult> ModificarUsuarios(AdministrarUsuariosVM modelo, string _method) // Método para modificar un usuario y sus vehículos
        {
            if(_method == "PUT") // Verifica si el método es PUT
            {
                var usuario = await _appDBcontext.Usuarios.Where(u => u.correo == modelo.correo).FirstOrDefaultAsync(); // Obtiene el usuario con el correo especificado
                if (usuario == null) return NotFound(); // Si no se encuentra el usuario, devuelve 404

                usuario.nombre = modelo.nombre;
                usuario.apellido = modelo.apellido;
                usuario.correo = modelo.correo;
                usuario.password = modelo.password;
                usuario.role = modelo.role;
                _appDBcontext.Usuarios.Update(usuario); // Actualiza los datos del usuario

                var id_usuario = await _appDBcontext.Usuarios.Where(u => u.correo == modelo.correo).Select(u => u.id_usuario).FirstOrDefaultAsync(); // Obtiene el id del usuario
                var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == id_usuario).ToListAsync(); // Obtiene los vehículos asociados al usuario
                if (vehiculos.Count > 0) // Verifica si hay vehículos en la lista
                {
                    if (!string.IsNullOrEmpty(modelo.placa1) && !string.IsNullOrEmpty(modelo.tipo_vehiculo1)) // Verifica si los campos de placa y tipo de vehículo no están vacíos
                    {
                        vehiculos[0].placa = modelo.placa1;
                        vehiculos[0].tipo_vehiculo = modelo.tipo_vehiculo1;
                        _appDBcontext.Vehiculos.Update(vehiculos[0]); // Actualiza los datos del vehículo
                    }
                }
                if (vehiculos.Count > 1) // Verifica si hay más de un vehículo en la lista
                {
                    if (!string.IsNullOrEmpty(modelo.placa2) && !string.IsNullOrEmpty(modelo.tipo_vehiculo2)) // Verifica si los campos de placa y tipo de vehículo no están vacíos
                    {
                        vehiculos[1].placa = modelo.placa2;
                        vehiculos[1].tipo_vehiculo = modelo.tipo_vehiculo2;
                        _appDBcontext.Vehiculos.Update(vehiculos[1]); // Actualiza los datos del vehículo
                    }
                }
                await _appDBcontext.SaveChangesAsync();
                return RedirectToAction("AdministrarUsuarios"); // Redirige a la vista de administración de usuarios
            }
            return BadRequest(); // Si el método no es PUT, devuelve un error 400
        }

       

        [HttpDelete]
        public async Task<IActionResult> AdministrarUsuarios(int id_usuario) // Método para eliminar un usuario y sus vehículos
        {
            var usuario = await _appDBcontext.Usuarios.FindAsync(id_usuario); // Obtiene el usuario con el id especificado
            if (usuario == null) return NotFound();

            var vehiculos = await _appDBcontext.Vehiculos.Where(v => v.id_usuario == usuario.id_usuario).ToListAsync(); // Obtiene los vehículos asociados al usuario
            if (vehiculos.Any()) // Verifica si hay vehículos en la lista
            {
                _appDBcontext.Vehiculos.RemoveRange(vehiculos);// Elimina los vehículos asociados al usuario
            }

            _appDBcontext.Usuarios.Remove(usuario);
            await _appDBcontext.SaveChangesAsync();
            return RedirectToAction("AdministrarUsuarios"); // Redirige a la vista de administración de usuarios
        }
        // Esta presentando errores----------------------------------------------

        [HttpGet]
        public async Task<IActionResult> PerfilUsuario() // Método para mostrar la vista de perfil de usuario
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

        [HttpPost]
        public async Task<IActionResult> PerfilUsuario(PerfilVM modelo) // Método para modificar los datos de un usuario y sus vehículos
    
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
        public IActionResult LogIn() // Método para mostrar la vista de inicio de sesión
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home"); // Si el usuario ya está autenticado, redirige a la página principal
            return View(); // Devuelve la vista
        }

        [HttpPost] // Método para autenticar un usuario
        public async Task<IActionResult> LogIn(LogInVM modelo)
        {
            if (!ModelState.IsValid) // Verifica si el modelo es válido
            {
                ViewData["Mensaje"] = "Datos inválidos.";
                return View();
            }

            Usuario? usuario_encontrado = await _appDBcontext.Usuarios// Busca el usuario en la base de datos
                .FirstOrDefaultAsync(u => u.correo == modelo.correo);
            TempData["UserEmail"] = usuario_encontrado.correo; // Guarda el correo del usuario en la cookie
            TempData["UserRole"] = usuario_encontrado.role; // Guarda el rol del usuario en la cookie

            // Verificación de existencia y contraseña
            if (usuario_encontrado == null || usuario_encontrado.password != EncryptPassword(modelo.password))
            {
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos.";
                return View();
            }

            // Validación de cambio de contraseña pendiente
            if (usuario_encontrado.RequiereCambioContrasena)
            {
                ViewData["Mensaje"] = "Debe cambiar su contraseña antes de continuar.";
                return RedirectToAction("CambiarContrasena", "Acceso", new { correo = usuario_encontrado.correo });
            }

            // Creación de Claims para autenticación
            List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario_encontrado.correo),
            new Claim(ClaimTypes.Role, usuario_encontrado.role)
        };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties authProperties = new AuthenticationProperties { AllowRefresh = true };

            if (usuario_encontrado.role == "Seguridad") // Si el usuario es de rol de seguridad, redirige a la selección de parqueo
            {
                ViewData["Mensaje"] = "Debe seleccionar el parqueo al que está asignado.";
                return RedirectToAction("SeleccionarParqueo", "Acceso");
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties); // Autentica al usuario

            return RedirectToAction("Index", "Home"); // Redirige a la página principal
        }

        [HttpGet]
        public IActionResult CambiarContrasena(string correo) // Método para mostrar la vista de cambio de contraseña
        {
            if (string.IsNullOrEmpty(correo))
            {
                return NotFound(); // Si no hay correo, devuelve 404
            }

            var model = new Cambiar_contrasenaVM
            {
                Correo = correo // Add the email to the model
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarContrasena(Cambiar_contrasenaVM modelo) // Método para cambiar la contraseña
        {
            if (!ModelState.IsValid) // Verifica si el modelo es válido
            {
                ViewData["Mensaje"] = "Datos inválidos.";
                return View(modelo);
            }

            if (modelo.NuevaContrasena != modelo.ConfirmarContrasena) // Verifica que las contraseñas coincidan
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View(modelo);
            }

            var correo = modelo.Correo; // Obtiene el correo del modelo
            if (correo == null)
            {
                return NotFound();
            }

            Usuario? usuario = await _appDBcontext.Usuarios // Busca el usuario en la base de datos
                .FirstOrDefaultAsync(u => u.correo == correo);

            if (usuario == null) // Verifica si el usuario existe
            {
                return NotFound();
            }

            // Verifica que la contraseña actual coincida
            if (usuario.password != EncryptPassword(modelo.ContrasenaActual)) // Verifica que la contraseña actual coincida
            {
                ViewData["Mensaje"] = "La contraseña actual es incorrecta.";
                return View(modelo);
            }

            // Cambia la contraseña y actualiza el estado de "RequiereCambioContrasena"
            usuario.password = modelo.NuevaContrasena; 
            usuario.RequiereCambioContrasena = false;
            _appDBcontext.Usuarios.Update(usuario);
            await _appDBcontext.SaveChangesAsync();

            return RedirectToAction("LogIn", "Acceso"); // Redirige a la vista de inicio de sesión
        }       


        private string EncryptPassword(string password) // Método para encriptar la contraseña
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create()) // Crea un objeto SHA256
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); // Convierte la contraseña a bytes y la encripta
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower(); // Devuelve la contraseña encriptada
            }
        }


        [HttpGet]
        public IActionResult SeleccionarParqueo() // Método para mostrar la vista de selección de parqueo
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home"); // Si el usuario ya está autenticado, redirige a la página principal

            var parqueos = _appDBcontext.Parqueos.ToList(); // Obtiene la lista de parqueos
            ViewData["Parqueos"] = parqueos; // Asigna la lista de parqueos a la vista
            ViewData["UserEmail"] = TempData["UserEmail"]; // Obtiene el correo del usuario
            ViewData["UserRole"] = TempData["UserRole"]; // Obtiene el rol del usuario

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeleccionarParqueo(string nombre_parqueo, string UserEmail, string UserRole) // Método para asignar un parqueo al usuario con rol de seguridad
        {
            if (nombre_parqueo == null) return NotFound(); // Si no hay parqueo, devuelve 404

            if (UserEmail == null || UserRole == null) return Unauthorized(); // Si no hay correo o rol, devuelve 401

            // Add the assigned parqueo to the user's claims
            List<Claim> claims = new List<Claim> // Crea una lista de claims con el correo, rol y nombre del parqueo
                {
                    new Claim(ClaimTypes.Name, UserEmail),
                    new Claim(ClaimTypes.Role, UserRole),
                    new Claim(ClaimTypes.StreetAddress, nombre_parqueo)
                };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // Crea un objeto ClaimsIdentity
            AuthenticationProperties authProperties = new AuthenticationProperties // Crea un objeto AuthenticationProperties
            {
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties); // Autentica al usuario

            return RedirectToAction("Index", "Home");// Redirige a la página principal
        }

        [HttpGet]
        public async Task<IActionResult> Parqueos()
        {
            var parqueos = await _appDBcontext.Parqueos
                .Include(p => p.Espacios) // Incluye los espacios relacionados
                .ToListAsync();

            var viewModel = parqueos.Select(p => new ParqueosConEspaciosVM
            {
                id_parqueo = p.id_parqueo,
                nombre_parqueo = p.nombre_parqueo,
                ubicacion = p.ubicacion,
                Espacios = p.Espacios
                    .GroupBy(e => e.tipo_espacio)
                    .Select(g => new EspaciosVM
                    {
                        tipo_espacio = g.Key,
                        cantidad = g.Count(),
                        disponibilidad = g.Any(e => e.disponibilidad) // Muestra si hay al menos un espacio disponible
                    }).ToList()
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CrearParqueo(ParqueosConEspaciosVM modelo)
        {
            var parqueo = new Parqueo
            {
                nombre_parqueo = modelo.nombre_parqueo,
                ubicacion = modelo.ubicacion,
                Espacios = modelo.Espacios.Select(e => new Espacio
                {
                    tipo_espacio = e.tipo_espacio,
                    disponibilidad = true // Por defecto, los nuevos espacios están disponibles
                }).ToList()
            };

            await _appDBcontext.Parqueos.AddAsync(parqueo);
            await _appDBcontext.SaveChangesAsync();

            return RedirectToAction("Parqueos");
        }

        [HttpPost]
        public async Task<IActionResult> EditarEspacios(int parqueoId, List<EspaciosVM> espaciosActualizados)
        {
            var parqueo = await _appDBcontext.Parqueos
                .Include(p => p.Espacios)
                .FirstOrDefaultAsync(p => p.id_parqueo == parqueoId);

            if (parqueo == null) return NotFound();

            // Actualizar espacios
            parqueo.Espacios.Clear(); // Elimina los espacios existentes
            parqueo.Espacios = espaciosActualizados.Select(e => new Espacio
            {
                tipo_espacio = e.tipo_espacio,
                disponibilidad = true // Nuevos espacios están disponibles por defecto
            }).ToList();

            _appDBcontext.Parqueos.Update(parqueo);
            await _appDBcontext.SaveChangesAsync();

            return RedirectToAction("Parqueos");
        }

    }

}
