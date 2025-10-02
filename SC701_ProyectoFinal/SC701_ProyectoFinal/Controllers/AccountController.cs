using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class AccountController : Controller
    {

        private static readonly List<User> _users = new();

        [HttpGet]
        public IActionResult Register()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid)
                return View(model);

            if (_users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Este correo ya está registrado.");
                return View(model);
            }

            // Guardar en memoria
            model.Id = _users.Count + 1;
            _users.Add(model);

            TempData["Ok"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Register));
        }
    }
}
