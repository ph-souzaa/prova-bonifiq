namespace ProvaPub.Services.Interfaces
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
