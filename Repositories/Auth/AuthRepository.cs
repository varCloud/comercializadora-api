using System.Data;
using Dapper;
using comercializadora_api.Models.Auth;
using comercializadora_api.Models.Common;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories
{
    /// <summary>
    /// Repositorio de autenticación. Envuelve SP_VALIDA_CONTRASENA con Dapper.
    /// El SP devuelve, en éxito (status = 200), cuatro resultsets:
    ///   1) cabecera (status, mensaje)  2) sesión  3) permisos  4) datos de empresa.
    /// En error devuelve solo la cabecera.
    /// </summary>
    public sealed class AuthRepository : BaseRepository, IAuthRepository
    {
        public AuthRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<Notificacion<Sesion>> ValidarUsuarioAsync(LoginRequest login)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@usuario", login.Usuario);
            parametros.Add("@contrasena", login.Contrasena);
            parametros.Add("@macAdress", login.MacAdress);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_VALIDA_CONTRASENA", parametros, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Sesion>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
            {
                notificacion.Mensaje = "Datos incorrectos";
                return notificacion;
            }

            var sesion = await multi.ReadFirstOrDefaultAsync<Sesion>();
            if (sesion is null)
            {
                notificacion.Estatus = -1;
                notificacion.Mensaje = "Datos incorrectos";
                return notificacion;
            }

            sesion.PermisosModulo = (await multi.ReadAsync<Permiso>()).ToList();

            var empresa = await multi.ReadFirstOrDefaultAsync<EmpresaComprobante>();
            if (empresa is not null)
            {
                sesion.DomicilioEmpresa = empresa.Domicilio;
                sesion.TelefonoEmpresa = empresa.Telefono;
                sesion.RfcEmpresa = empresa.Rfc;
            }

            notificacion.Modelo = sesion;
            return notificacion;
        }
    }
}
