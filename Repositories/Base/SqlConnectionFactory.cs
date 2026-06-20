using System.Data;
using Microsoft.Data.SqlClient;

namespace comercializadora_api.Repositories.Base
{
    /// <summary>
    /// Fábrica de conexiones SQL Server. Lee la cadena "DefaultConnection".
    /// </summary>
    public sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Falta la cadena de conexión 'DefaultConnection'. " +
                    "Configúrala con User Secrets o variables de entorno.");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
