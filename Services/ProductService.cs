using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services
{
	public class ProductService : IProductService
    {
		TestDbContext _ctx;

		public ProductService(TestDbContext ctx)
		{
			_ctx = ctx;
		}

        public PagedResponse<Product> ListProducts(int page, int pageSize = 10)
        {
            if (page < 1) page = 1;

            var query = _ctx.Products.AsNoTracking().OrderBy(p => p.Id);
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResponse<Product>
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = total,
                Data = items,
            };
        }
    }
}
