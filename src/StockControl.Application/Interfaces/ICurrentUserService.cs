namespace StockControl.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool EstaAutenticado { get; }
}
