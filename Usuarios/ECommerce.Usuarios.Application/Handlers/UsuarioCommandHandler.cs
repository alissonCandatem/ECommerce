using ECommerce.Mediator.Abstractions;
using ECommerce.Mediator.Shared;
using ECommerce.Usuarios.Application.Commands;
using ECommerce.Usuarios.Application.Interfaces;
using ECommerce.Usuarios.Application.Responses;
using ECommerce.Usuarios.Domain.Entities.Usuario;
using ECommerce.Usuarios.Domain.Interfaces.Usuario;

namespace ECommerce.Usuarios.Application.Handlers
{
  public class UsuarioCommandHandler :
    ICommandHandler<CriarUsuarioCommand, ResultNotification>,
    ICommandHandler<LoginCommand, Result<LoginResponse>>,
    ICommandHandler<RefreshTokenCommand, Result<LoginResponse>>
  {
    private readonly IUsuarioRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly INotificationContext _notification;

    public UsuarioCommandHandler(
      IUsuarioRepository repository,
      IJwtService jwtService,
      INotificationContext notification
    )
    {
      _repository = repository;
      _jwtService = jwtService;
      _notification = notification;
    }

    public async Task<ResultNotification> Handle(CriarUsuarioCommand command, CancellationToken cancellationToken)
    {
      var emailExiste = await _repository.ExisteEmailAsync(command.Email, cancellationToken);

      if (emailExiste)
      {
        _notification.AddConflict("Email já cadastrado");
        return ResultNotification.Fail(_notification);
      }

      var senhaHash = BCrypt.Net.BCrypt.HashPassword(command.Senha);

      var user = User.Criar(command.Nome, command.Email, senhaHash);

      await _repository.AdicionarAsync(user, cancellationToken);

      return ResultNotification.Ok();
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
      var user = await _repository.ObterPorEmailAsync(command.Email, cancellationToken);

      if (user == null || !BCrypt.Net.BCrypt.Verify(command.Senha, user.SenhaHash))
        return Result<LoginResponse>.Fail(Error.Unauthorized("Email ou senha inválidos"));

      var accessToken = _jwtService.GerarAccessToken(user);
      var refreshToken = _jwtService.GerarRefreshToken();
      var expiry = DateTime.UtcNow.AddDays(7);

      user.DefinirRefreshToken(refreshToken, expiry);
      _repository.Atualizar(user);

      return Result<LoginResponse>.Ok(new LoginResponse
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
      });
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
      var userId = _jwtService.ObterUserIdToken(command.AccessToken);
      if (userId == null)
        return Result<LoginResponse>.Fail(Error.Unauthorized("Token inválido"));

      var user = await _repository.ObterPorIdAsync(userId.Value, cancellationToken);
      if (user == null || !user.RefreshTokenValido(command.RefreshToken))
        return Result<LoginResponse>.Fail(Error.Unauthorized("Refresh token inválido ou expirado"));

      var accessToken = _jwtService.GerarAccessToken(user);
      var refreshToken = _jwtService.GerarRefreshToken();

      user.DefinirRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
      _repository.Atualizar(user);

      return Result<LoginResponse>.Ok(new LoginResponse
      {
        AccessToken = accessToken,
        RefreshToken = refreshToken,
        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
      });
    }
  }
}
