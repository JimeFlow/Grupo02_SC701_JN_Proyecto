using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ProyectoAPI.Services
{
    public class SancionCleanUpService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SancionCleanUpService> _logger;

        public SancionCleanUpService(IConfiguration configuration, ILogger<SancionCleanUpService> logger)
        {
            _config = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcesarSancionesCumplidas();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);  // Cada 2 horas
            }
        }

        private async Task ProcesarSancionesCumplidas()
        {
            try
            {
                using var connection = new SqlConnection(_config["ConnectionStrings:BDConnection"]);

                var usuarios = connection.Query<int>(
                    "VerificarSancionesCumplidas",
                    commandType: CommandType.StoredProcedure
                ).ToList();

                foreach (var usuarioId in usuarios)
                {
                    connection.Execute(
                        "InactivarSancionUsuario",
                        new { Id_Usuario = usuarioId },
                        commandType: CommandType.StoredProcedure
                    );

                    _logger.LogInformation($"Sanción inactivada para usuario {usuarioId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar sanciones cumplidas");
            }
        }
    }
}
