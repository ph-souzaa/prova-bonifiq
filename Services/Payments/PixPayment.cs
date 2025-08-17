using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services.Payments
{
    public class PixPayment : IPayment
    {
        public string Name => "pix";
        public Task<bool> Pay(decimal value, int customerId)
        {
            return Task.FromResult(true);
        }
    }
}
