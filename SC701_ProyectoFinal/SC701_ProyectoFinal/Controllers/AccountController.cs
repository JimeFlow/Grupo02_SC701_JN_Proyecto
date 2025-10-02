using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;

using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class AccountController : Controller
    {

        //private readonly SignInManager<IdentityUser> _signInManager;
        //public AccountController(SignInManager<IdentityUser> signInManager)
        //{
        //    _signInManager = signInManager;
        //}

        [HttpGet]
        public IActionResult Login(/*string returnUrl = null*/)
        {
            //ViewData["ReturnUrl"] = returnUrl;
            return View();


        //private static readonly List<User> _users = new();

        //[HttpGet]
        //public IActionResult Register()
        //{
          //  return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        //{
        //    ViewData["ReturnUrl"] = returnUrl;
        //    if (ModelState.IsValid)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        //        if (result.Succeeded)
        //        {
        //            if (Url.IsLocalUrl(returnUrl))
        //            {
        //                return Redirect(returnUrl);
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index", "Home");
        //            }
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        //        }
        //    }
        //    return View(model);
        //}

        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool validCredentials = ValidateUser(model.Email, model.Password);

            if (validCredentials)
            {
                HttpContext.Session.SetString("Usuario", model.Email);
                return RedirectToAction("Index", "Home"); // Redirect to dashboard or home page
            }

            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);

        }

        private bool ValidateUser(string correo, string contrasenna)
        {
            return correo == "admin@correo.com" && contrasenna == "12345"; /*Temporal*/
        }
        
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
