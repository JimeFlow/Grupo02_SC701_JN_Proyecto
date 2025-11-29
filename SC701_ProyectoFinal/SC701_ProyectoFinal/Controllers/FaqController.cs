using Microsoft.AspNetCore.Mvc;
using SC701_ProyectoFinal.Models;

namespace SC701_ProyectoFinal.Controllers
{
    public class FaqController : Controller
    {

        public IActionResult Index()
        {
            var preguntas = new List<PreguntaFrecuente>
            {
                new PreguntaFrecuente
                {
                    Pregunta = "¿Cómo puedo reservar un libro?",
                    Respuesta = "Debe iniciar sesión, buscar el libro y presionar el botón 'Reservar'."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Cuántos libros puedo tener prestados a la vez?",
                    Respuesta = "Puede tener un máximo de 3 préstamos activos simultáneamente."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Qué pasa si devuelvo un libro tarde?",
                    Respuesta = "El sistema calculará automáticamente una multa por cada día de atraso."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Puedo renovar un préstamo?",
                    Respuesta = "Sí, siempre que el libro no tenga una reserva activa."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Dónde veo mis préstamos activos?",
                    Respuesta = "Desde el menú principal seleccione 'Mis Préstamos'."
                },
                new PreguntaFrecuente
                {
                       Pregunta = "¿Cómo puedo saber la fecha exacta de devolución de mis préstamos?",
                       Respuesta = "En la sección 'Mis Préstamos' podrá ver la fecha límite de devolución y el estado actualizado de cada ejemplar."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Puedo eliminar un comentario que ya hice en un libro?",
                    Respuesta = "Por el momento no es posible eliminar comentarios desde la plataforma. Para solicitar cambios, comuníquese con el administrador."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿El sistema envía recordatorios antes de la fecha de devolución?",
                    Respuesta = "Sí, el sistema puede enviar recordatorios automáticos por correo electrónico dependiendo de la configuración de la biblioteca."
                },
                new PreguntaFrecuente
                {
                    Pregunta = "¿Qué hago si no encuentro un libro en el catálogo pero sé que la biblioteca lo tiene?",
                    Respuesta = "Es posible que el libro esté en proceso de registro o revisión de inventario. Consulte con el bibliotecario para verificar su disponibilidad."
                }

            };

            return View(preguntas);
        }
    }
}