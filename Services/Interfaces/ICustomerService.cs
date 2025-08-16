using ProvaPub.Models;
using ProvaPub.Models.Responses;

namespace ProvaPub.Services.Interfaces
{
    public interface ICustomerService
    {
        PagedResponse<Customer> ListCustomers(int page, int pageSize = 10);

        Task<bool> CanPurchase(int customerId, decimal purchaseValue);
    }
}
