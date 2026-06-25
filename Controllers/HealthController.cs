using System.Data;
using comercializadora_api.Repositories.Base;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Endpoints de salud para verificar que la API se desplegó y está activa.
    /// Son <see cref="AllowAnonymousAttribute"/> a propósito: sirven para validar el
    /// despliegue (p.ej. en IIS) sin necesitar un JWT. No exponen datos sensibles.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HealthController(IDbConnectionFactory connectionFactory)
            => _connectionFactory = connectionFactory;

        /// <summary>
        /// Liveness simple: si responde, el proceso de la API está vivo.
        /// GET /api/health  ->  { status: "ready to work", ... }
        /// </summary>
        [HttpGet]
        public object Get() => new
        {
            status = "ready to work",
            service = "comercializadora-api",
            environment = HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>().EnvironmentName,
            utcNow = DateTime.UtcNow
        };

        /// <summary>
        /// Readiness con chequeo de base de datos: abre una conexión y ejecuta SELECT 1.
        /// Devuelve 200 si la BD responde, 503 si no. Útil para confirmar que la cadena de
        /// conexión de producción es correcta tras el deploy.
        /// GET /api/health/db
        /// </summary>
        [HttpGet("db")]
        public async Task<IActionResult> GetDb()
        {
            try
            {
                using IDbConnection conn = _connectionFactory.CreateConnection();
                var result = await conn.ExecuteScalarAsync<int>("SELECT 1");
                return Ok(new
                {
                    status = "ready to work",
                    database = result == 1 ? "up" : "unexpected",
                    utcNow = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    status = "degraded",
                    database = "down",
                    error = ex.Message,
                    utcNow = DateTime.UtcNow
                });
            }
        }
    }
}
