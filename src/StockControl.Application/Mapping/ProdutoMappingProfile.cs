using AutoMapper;
using StockControl.Application.DTOs;
using StockControl.Domain.Aggregates;

namespace StockControl.Application.Mapping;

public sealed class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<Produto, ProdutoDto>()
            .ForMember(dto => dto.Codigo, opt => opt.MapFrom(p => p.Codigo.Value))
            .ForMember(dto => dto.CodigoBarras, opt => opt.MapFrom(p => p.CodigoBarras != null ? p.CodigoBarras.Value : null))
            .ForMember(dto => dto.Preco, opt => opt.MapFrom(p => p.Preco.Amount))
            .ForMember(dto => dto.Estoque, opt => opt.MapFrom(p => p.Estoque.Value))
            .ForMember(dto => dto.EstoqueMinimo, opt => opt.MapFrom(p => p.EstoqueMinimo.Value))
            .ForMember(dto => dto.AbaixoDoMinimo, opt => opt.MapFrom(p => p.EstoqueAbaixoDoMinimo()));
    }
}
