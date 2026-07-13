using FluentValidation;

namespace StockControl.Application.Commands.Produtos;

public sealed class AtualizarProdutoCommandValidator : AbstractValidator<AtualizarProdutoCommand>
{
    public AtualizarProdutoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O código é obrigatório.")
            .Length(3, 30);

        RuleFor(x => x.Preco)
            .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser negativo.");

        RuleFor(x => x.Estoque)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EstoqueMinimo)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CategoriaId)
            .NotEmpty().WithMessage("A categoria é obrigatória.");
    }
}
