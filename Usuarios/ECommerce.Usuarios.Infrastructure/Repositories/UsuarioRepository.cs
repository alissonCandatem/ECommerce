using ECommerce.Usuarios.Domain.Entities.Usuario;
using ECommerce.Usuarios.Domain.Interfaces.Usuario;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Usuarios.Infrastructure.Repositories
{
  public sealed class UsuarioRepository : IUsuarioRepository
  {
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
      _context = context;
    }

    public async Task<User?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<User?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(x => x.Email == email, cancellationToken);

    public async Task AdicionarAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Atualizar(User user)
        => _context.Users.Update(user);
  }
}
