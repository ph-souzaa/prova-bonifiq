using ProvaPub.Models;
using ProvaPub.Models.Responses;

namespace ProvaPub.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Response<Order>> PayOrder(string paymentMethod, decimal paymentValue, int customerId);
    }
}
