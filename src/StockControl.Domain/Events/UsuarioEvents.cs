namespace StockControl.Domain.Events;

public sealed class UsuarioCriadoEvent : DomainEvent
{
    public UsuarioCriadoEvent(Guid usuarioId, string email)
    {
        UsuarioId = usuarioId;
        Email = email;
    }

    public Guid UsuarioId { get; }
    public string Email { get; }
}

public sealed class UsuarioAlterouSenhaEvent : DomainEvent
{
    public UsuarioAlterouSenhaEvent(Guid usuarioId)
    {
        UsuarioId = usuarioId;
    }

    public Guid UsuarioId { get; }
}
