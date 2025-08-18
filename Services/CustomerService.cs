using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services
{
    public class CustomerService : ICustomerService
    {
        TestDbContext _ctx;
        IClock _clock;

        public CustomerService(TestDbContext ctx, IClock clock)
        {
            _ctx = ctx;
            _clock = clock;
        }

        public PagedResponse<Customer> ListCustomers(int page, int pageSize = 10)
        {
            if (page < 1) page = 1;

            var query = _ctx.Customers.AsNoTracking().OrderBy(c => c.Id);
            var total = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResponse<Customer>
            {
                Data = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = total,
            };
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));

            if (purchaseValue <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseValue));

            //Business Rule: Non registered Customers cannot purchase
            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null) throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            //Business Rule: A customer can purchase only a single time per month
            var baseDate = _clock.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _ctx.Orders.CountAsync(s => s.CustomerId == customerId && s.OrderDate >= baseDate);
            if (ordersInThisMonth > 0)
                return false;


            //Business Rule: A customer that never bought before can make a first purchase of maximum 100,00
            var haveBoughtBefore = await _ctx.Customers.CountAsync(s => s.Id == customerId && s.Orders.Any());
            if (haveBoughtBefore == 0 && purchaseValue > 100)
                return false;

            //Business Rule: A customer can purchases only during business hours and working days
            if (_clock.UtcNow.Hour < 8 || _clock.UtcNow.Hour > 18 || _clock.UtcNow.DayOfWeek == DayOfWeek.Saturday || _clock.UtcNow.DayOfWeek == DayOfWeek.Sunday)
                return false;


            return true;
        }

    }
}
