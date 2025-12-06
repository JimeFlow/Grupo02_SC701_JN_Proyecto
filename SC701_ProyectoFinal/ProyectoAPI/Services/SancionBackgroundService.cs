using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ProyectoAPI.Services
{
    public class SancionBackgroundService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SancionBackgroundService> _logger;

        public SancionBackgroundService(IConfiguration config, ILogger<SancionBackgroundService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcesarSanciones();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); //3 horas
            }
        }


        private async Task ProcesarSanciones()
        {
            try
            {
                using var connection = new SqlConnection(_config["ConnectionStrings:BDConnection"]);

                var movimientos = connection.Query<dynamic>("ObtenerMovimientosAtrasados", commandType: CommandType.StoredProcedure);

                foreach (var mov in movimientos)
                {
                    int usuarioId = mov.Id_Usuario;

                    int tieneSancion = connection.ExecuteScalar<int>(
                        "UsuarioTieneSancionActiva",
                        new { Id_Usuario = usuarioId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (tieneSancion == 0)
                    {
                        
                        connection.Execute(
                            "RegistrarSancion",
                            new { Id_Usuario = usuarioId },
                            commandType: CommandType.StoredProcedure
                        );

                        _logger.LogInformation($"Sanción creada para usuario {usuarioId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando sanciones");
            }
        }
    }
}
