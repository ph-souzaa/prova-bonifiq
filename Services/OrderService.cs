using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services
{
	public class OrderService : IOrderService
	{
        TestDbContext _ctx;
        IEnumerable<IPayment> _payment;

        public OrderService(TestDbContext ctx, IEnumerable<IPayment> paymentStrategies)
        {
            _ctx = ctx;
            _payment = paymentStrategies;
        }


        public async Task<Response<Order>> PayOrder(string paymentMethod, decimal paymentValue, int customerId)
		{
            var strategy = _payment.FirstOrDefault(p =>
                p.Name.Equals(paymentMethod, StringComparison.OrdinalIgnoreCase));

            if (strategy is null)
                return Response<Order>.Fail($"Método de pagamento '{paymentMethod}' não suportado.");

            var ok = await strategy.Pay(paymentValue, customerId);
            if (!ok)
                return Response<Order>.Fail("Falha ao processar o pagamento.");

            var order = new Order
            {
                CustomerId = customerId,
                Value = paymentValue,
                OrderDate = DateTime.UtcNow
            };

            var saved = await InsertOrder(order);
            return Response<Order>.Ok(saved, "Pedido criado com sucesso.");

        }

        private async Task<Order> InsertOrder(Order order)
        {
            var entry = await _ctx.Orders.AddAsync(order);
            await _ctx.SaveChangesAsync();
            return entry.Entity;
        }
    }
}
