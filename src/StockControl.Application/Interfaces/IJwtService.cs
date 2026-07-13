using StockControl.Domain.Aggregates;

namespace StockControl.Application.Interfaces;

public interface IJwtService
{
    string GerarAccessToken(Usuario usuario);
    string GerarRefreshToken();
}
