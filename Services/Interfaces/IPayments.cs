namespace ProvaPub.Services.Interfaces
{
    public interface IPayment
    {
        string Name { get; }
        Task<bool> Pay(decimal value, int customerId);
    }
}
