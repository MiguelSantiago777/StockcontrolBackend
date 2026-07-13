using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Entregador : AggregateRoot
{
    private Entregador() { } // EF Core

    private Entregador(string nome, Cpf cpf, Telefone telefone, Guid usuarioId)
    {
        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
        UsuarioId = usuarioId;
        Status = StatusEntregador.Disponivel;
    }

    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public Telefone Telefone { get; private set; } = null!;
    public Guid UsuarioId { get; private set; }
    public StatusEntregador Status { get; private set; }
    public CoordenadaGeografica? UltimaPosicao { get; private set; }
    public DateTime? PosicaoAtualizadaEm { get; private set; }

    public static Result<Entregador> Criar(string? nome, Cpf cpf, Telefone telefone, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return Result.Failure<Entregador>(Error.Validation("Entregador.Nome", "O nome é obrigatório."));

        return new Entregador(nome.Trim(), cpf, telefone, usuarioId);
    }

    public Result AtualizarPosicao(double latitude, double longitude)
    {
        var coordenada = CoordenadaGeografica.Create(latitude, longitude);
        if (coordenada.IsFailure) return Result.Failure(coordenada.Error);

        UltimaPosicao = coordenada.Value;
        PosicaoAtualizadaEm = DateTime.UtcNow;
        MarkAsUpdated();
        return Result.Success();
    }

    public void IniciarEntrega() { Status = StatusEntregador.EmEntrega; MarkAsUpdated(); }
    public void FicarDisponivel() { Status = StatusEntregador.Disponivel; MarkAsUpdated(); }
    public void FicarIndisponivel() { Status = StatusEntregador.Indisponivel; MarkAsUpdated(); }
}
