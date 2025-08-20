using ProvaPub.Models;
using ProvaPub.Models.Responses;

namespace ProvaPub.Services.Interfaces
{
    public interface IProductService
    {
        PagedResponse<Product> ListProducts(int page, int pageSize = 10);
    }
}
