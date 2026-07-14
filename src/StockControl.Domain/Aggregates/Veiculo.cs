using StockControl.Domain.Common;
using StockControl.Domain.Enums;
using StockControl.Domain.ValueObjects;

namespace StockControl.Domain.Aggregates;

public sealed class Veiculo : AggregateRoot
{
    private Veiculo() { } // EF Core

    private Veiculo(Placa placa, TipoVeiculo tipo, string? modelo)
    {
        Placa = placa;
        Tipo = tipo;
        Modelo = modelo;
    }

    public Placa Placa { get; private set; } = null!;
    public TipoVeiculo Tipo { get; private set; }
    public string? Modelo { get; private set; }

    public static Result<Veiculo> Criar(Placa placa, TipoVeiculo tipo, string? modelo) =>
        new Veiculo(placa, tipo, modelo?.Trim());

    public Result Atualizar(Placa placa, TipoVeiculo tipo, string? modelo)
    {
        Placa = placa;
        Tipo = tipo;
        Modelo = modelo?.Trim();
        MarkAsUpdated();
        return Result.Success();
    }
}
