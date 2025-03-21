using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ParqueoApp3.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace ParqueoApp3.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    [Authorize(Roles = "Administrador")]
    public IActionResult AdministrarUsuario()
    {
        return View();
    }
    [Authorize(Roles = "Administrador")]
    public IActionResult ModificarUsuarios()
    {
        return View();
    }
    public IActionResult PerfilUsuario()
    {
        return View();
    }

    [Authorize (Roles = "Administrador")]
    public IActionResult Parqueos()
    {
        return View();
    }

    [Authorize (Roles = "Seguridad")]
    public IActionResult Seguridad()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> Salir()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("LogIn", "Acceso");
    }
}
