using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class FaqController : Controller
    {

        public IActionResult Index()
        {
            var faqs = new List<Models.FaqViewModel>
            {
                new Models.FaqViewModel
                {
                    Question = "¿Cómo puedo crear una cuenta?",
                    Answer = "Para crear una cuenta, haz clic en el botón de registro y completa el formulario con tus datos."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cómo reservo un libro?",
                    Answer = "Debes iniciar sesión en el sistema, buscar el libro y hacer clic en 'Reservar'."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cuánto tiempo dura una reserva?",
                    Answer = "Generalmente entre 3 y 7 días hábiles, dependiendo de la política de la biblioteca."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Puedo renovar una reserva?",
                    Answer = "Sí, si el libro no ha sido solicitado por otro usuario. La renovación se hace desde tu perfil."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cuántos libros puedo reservar al mismo tiempo?",
                    Answer = "El límite varía, pero suele estar entre 2 y 5 libros por usuario."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Dónde debo devolver los libros?",
                    Answer = "En la misma biblioteca donde se realizó el préstamo, dentro del horario establecido."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Qué pasa si no devuelvo un libro a tiempo?",
                    Answer = "Se aplican multas por día de retraso. El monto depende del reglamento interno."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Dónde puedo pagar las multas?",
                    Answer = "En el área financiera de la institución o mediante el sistema en línea si está habilitado."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cómo recupero mi contraseña?",
                    Answer = "En la pantalla de inicio de sesión, haz clic en '¿Olvidaste tu contraseña?' y sigue los pasos."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cómo sé si tengo libros pendientes de devolución?",
                    Answer = "Inicia sesión en tu cuenta y revisa el historial de préstamos en tu perfil."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Puedo usar la biblioteca si no soy estudiante?",
                    Answer = "Algunas bibliotecas permiten acceso limitado a usuarios externos, pero no siempre se permite el préstamo."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Cómo accedo a la biblioteca digital?",
                    Answer = "Usa tu correo institucional y contraseña en el portal de acceso digital de la biblioteca."
                },
                new Models.FaqViewModel
                {
                    Question = "¿Puedo descargar libros digitales?",
                    Answer = "Depende del tipo de licencia. Algunos libros solo permiten lectura en línea."
                }
            };

            return View(faqs);
        }
    }
}
