using ECommerce.Usuarios.Domain.Events.Usuario;
using ECommerce.Usuarios.Domain.Primitives;

namespace ECommerce.Usuarios.Domain.Entities.Usuario
{
  public sealed class User : AggregateRoot
  {
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public Role Role { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime CriadoEm { get; private init; }

    private User(Guid id) : base(id) { }

    public static User Criar(string nome, string email, string senhaHash, Role role = Role.User)
    {
      var user = new User(Guid.NewGuid())
      {
        Nome = nome,
        Email = email,
        SenhaHash = senhaHash,
        Role = role,
        CriadoEm = DateTime.UtcNow
      };

      user.RaiseDomainEvent(new UsuarioCriadoEvent
      {
        UserId = user.Id,
        Email = email
      });

      return user;
    }

    public void AlterarSenha(string novaSenhaHash)
    {
      SenhaHash = novaSenhaHash;
      RaiseDomainEvent(new UsuarioSenhaAlteradaEvent { UserId = Id });
    }

    public void DefinirRefreshToken(string token, DateTime expiry)
    {
      RefreshToken = token;
      RefreshTokenExpiry = expiry;
    }

    public void RevogarRefreshToken()
    {
      RefreshToken = null;
      RefreshTokenExpiry = null;
    }

    public bool RefreshTokenValido(string token)
        => RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
  }
}
