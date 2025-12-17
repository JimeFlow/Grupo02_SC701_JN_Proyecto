using System.Data;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using ProyectoAPI.Models;
using Utils;

namespace ProyectoAPI.Services
{
    public class RecordatorioExpiracionService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly IHostEnvironment _environment;

        public RecordatorioExpiracionService(IConfiguration config, IHostEnvironment environment)
        {
            _config = config;
            _environment = environment;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                var ahora = DateTime.Now;
                var proximaEjecucion = DateTime.Today.AddDays(1).AddMinutes(1); 

                var delay = proximaEjecucion - ahora;

                if (delay.TotalMilliseconds > 0)
                    await Task.Delay(delay, stoppingToken); //se ejecuta a las 00:01 cada dia

                await ProcesarReservasPorVencer();
                //await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task ProcesarReservasPorVencer()
        {
            try
            {
                using var connection = new SqlConnection(_config["ConnectionStrings:BDConnection"]);

                var reservas = connection.Query<ReservaRecordatorioCorreoModel>(
                    "ReservasPorVencerPronto",
                    commandType: CommandType.StoredProcedure
                ).ToList();

                if (reservas.Count == 0)
                    return;

                var helper = new Helper();

                foreach (var reserva in reservas)
                {
                    await EnviarCorreoReserva(reserva, helper);
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Error interno {ex.Message}");
            }
        }

        private async Task EnviarCorreoReserva(ReservaRecordatorioCorreoModel reserva, Helper helper)
        {
            var ruta = Path.Combine(_environment.ContentRootPath, "PlantillasCorreo", "NotificacionReserva.html");
            var html = await File.ReadAllTextAsync(ruta, Encoding.UTF8);

            html = html.Replace("{{Usuario}}", reserva.Nombre)
                       .Replace("{{Libro}}", reserva.Titulo)
                       .Replace("{{FechaVencimiento}}", reserva.Fecha_Vencimiento.ToString("dd/MM/yyyy"));

            if (!string.IsNullOrEmpty(reserva.Correo))
            {
                helper.EnviarCorreo("Recordatorio de devolución", html, reserva.Correo);
            }
        }


    }
}
