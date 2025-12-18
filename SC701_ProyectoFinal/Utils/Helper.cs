using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Helper
    {
        private readonly string CorreoSMTP = "avalverde50252@ufide.ac.cr";
        private readonly string ContrasenaSMTP = ""; //AQUI SU CONTRASEÑA PARA QUE LES SIRVA!


        public void EnviarCorreo(string subject, string body, string destinatario)
        {

            if (string.IsNullOrEmpty(ContrasenaSMTP))
                return;

            var mensaje = new MailMessage
            {
                From = new MailAddress(CorreoSMTP),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(CorreoSMTP, ContrasenaSMTP),
                EnableSsl = true
            };

            smtp.Send(mensaje);
        }
    }
}
