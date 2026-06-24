using System.Globalization;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Usuarios;

namespace comercializadora_api.Services.Usuarios
{
    /// <summary>
    /// Implementación de la lógica de negocio de Usuarios. Ver <see cref="IUsuariosService"/>.
    /// </summary>
    public sealed class UsuariosService : IUsuariosService
    {
        private static readonly TextInfo TextInfo = CultureInfo.GetCultureInfo("es-MX").TextInfo;

        private readonly IUsuariosRepository _repository;

        public UsuariosService(IUsuariosRepository repository) => _repository = repository;

        public Task<RawPage<Usuario>> ListarAsync(UsuariosQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Usuario>> ObtenerPorIdAsync(int idUsuario)
            => _repository.ObtenerPorIdAsync(idUsuario);

        public async Task<Notificacion<string>> GuardarAsync(GuardarUsuarioRequest usuario)
        {
            // Normaliza nombre y apellidos a Title Case, igual que el DAO legado.
            usuario.Nombre = ToTitleCase(usuario.Nombre);
            usuario.ApellidoPaterno = ToTitleCase(usuario.ApellidoPaterno);
            usuario.ApellidoMaterno = ToTitleCase(usuario.ApellidoMaterno);

            // En edición, si la contraseña llega vacía, conserva la actual (no la sobreescribe
            // con blanco). El SP siempre setea contrasena, así que la resolvemos aquí.
            if (usuario.IdUsuario > 0 && string.IsNullOrWhiteSpace(usuario.Contrasena))
            {
                var actual = await _repository.ObtenerPorIdAsync(usuario.IdUsuario);
                usuario.Contrasena = actual.Modelo?.Contrasena;
            }

            return await _repository.GuardarAsync(usuario);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idUsuario, bool activo)
            => _repository.CambiarEstatusAsync(idUsuario, activo);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRolesAsync()
            => _repository.ObtenerRolesAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync()
            => _repository.ObtenerSucursalesAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
            => _repository.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);

        private static string ToTitleCase(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : TextInfo.ToTitleCase(value.ToLower(CultureInfo.CurrentCulture));
    }
}
