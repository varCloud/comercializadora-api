using System.Data;

namespace comercializadora_api.Repositories.Base
{
    /// <summary>
    /// Crea conexiones a la base de datos. Permite mockear el acceso a datos en pruebas.
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
