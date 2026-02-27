using ECommerce.Usuarios.Domain.Entities.Usuario;

namespace ECommerce.Usuarios.Domain.Interfaces.Usuario
{
  public interface IUsuarioRepository
  {
    Task<User?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(User user, CancellationToken cancellationToken = default);
    void Atualizar(User user);
  }
}
