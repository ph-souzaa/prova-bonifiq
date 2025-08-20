using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services.Payments
{
    public class CreditCardPayment : IPayment
    {
        public string Name => "creditcard";
        public Task<bool> Pay(decimal value, int customerId)
        {
            return Task.FromResult(true);
        }
    }
}
