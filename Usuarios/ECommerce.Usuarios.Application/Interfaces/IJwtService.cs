using ECommerce.Usuarios.Domain.Entities.Usuario;

namespace ECommerce.Usuarios.Application.Interfaces
{
  public interface IJwtService
  {
    string GerarAccessToken(User user);
    string GerarRefreshToken();
    Guid? ObterUserIdToken(string accessToken);
  }
}
