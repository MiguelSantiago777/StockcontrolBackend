using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.Events;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Usuario : AggregateRoot
{
    private Usuario() { } // EF Core

    private Usuario(string nome, Email email, SenhaHash senha, PerfilUsuario perfil)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
        Perfil = perfil;
    }

    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public SenhaHash Senha { get; private set; } = null!;
    public PerfilUsuario Perfil { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiraEm { get; private set; }

    public static Result<Usuario> Criar(string? nome, Email email, SenhaHash senha, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure<Usuario>(Error.Validation("Usuario.Nome", "O nome é obrigatório."));

        var usuario = new Usuario(nome.Trim(), email, senha, perfil);
        usuario.RaiseDomainEvent(new UsuarioCriadoEvent(usuario.Id, email.Value));
        return usuario;
    }

    public Result Atualizar(string? nome, Email email, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure(Error.Validation("Usuario.Nome", "O nome é obrigatório."));

        Nome = nome.Trim();
        Email = email;
        Perfil = perfil;
        MarkAsUpdated();
        return Result.Success();
    }

    public void AlterarSenha(SenhaHash novaSenha)
    {
        Senha = novaSenha;
        MarkAsUpdated();
        RaiseDomainEvent(new UsuarioAlterouSenhaEvent(Id));
    }

    public void DefinirRefreshToken(string token, DateTime expiraEm)
    {
        RefreshToken = token;
        RefreshTokenExpiraEm = expiraEm;
        MarkAsUpdated();
    }

    public void RevogarRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiraEm = null;
        MarkAsUpdated();
    }

    public bool RefreshTokenValido(string token) =>
        RefreshToken == token && RefreshTokenExpiraEm > DateTime.UtcNow;
}
