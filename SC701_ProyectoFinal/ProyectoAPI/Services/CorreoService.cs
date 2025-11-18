namespace ProyectoAPI.Services
{
    public class CorreoService
    {
        private readonly IConfiguration _configuration;

        public CorreoService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            // Lógica para enviar correo electrónico
            // Esta es una implementación simplificada y debe ser adaptada según las necesidades específicas
            using (var client = new System.Net.Mail.SmtpClient(_configuration["Email:SmtpHost"])) 
            {
                client.Port = int.Parse(_configuration["Email:SmtpPort"]);
                client.Credentials = new System.Net.NetworkCredential(_configuration["Email:SmtpUsername"], _configuration["Email:SmtpPassword"]);
                client.EnableSsl = true;

                var mailMessage = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(_configuration["Email:SmtpUsername"]),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(destinatario);
                client.Send(mailMessage);
            }
        }
    }
}
