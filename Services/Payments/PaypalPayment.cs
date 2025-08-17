using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services.Payments
{
    public class PaypalPayment : IPayment
    {
        public string Name => "paypal";
        public Task<bool> Pay(decimal value, int customerId)
        {
            return Task.FromResult(true);
        }
    }
}
