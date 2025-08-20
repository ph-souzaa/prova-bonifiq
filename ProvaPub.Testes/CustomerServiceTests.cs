using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;
using ProvaPub.Services.Interfaces;
using Xunit;

namespace ProvaPub.Testes
{
    /// <summary>
    /// Relógio de teste controlável para forçar horários/dias específicos.
    /// </summary>
    internal class TestClock : IClock
    {
        public DateTime Now { get; set; }
        public DateTime UtcNow => Now;
    }

    public class CustomerServiceTests
    {
        private static TestDbContext BuildContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .EnableSensitiveDataLogging()
                .Options;

            return new TestDbContext(options);
        }

        private static async Task SeedAsync(TestDbContext ctx, Action<(Customer c1, Customer c2, Customer c3)>? configure = null)
        {
            var c1 = new Customer { Id = 1, Name = "NoOrdersYet" };
            var c2 = new Customer { Id = 2, Name = "OldOrderOnly" };
            var c3 = new Customer { Id = 3, Name = "RecentOrder" };

            await ctx.Customers.AddRangeAsync(c1, c2, c3);
            await ctx.SaveChangesAsync();

            configure?.Invoke((c1, c2, c3));
            await ctx.SaveChangesAsync();
        }

        private static CustomerService CreateSut(TestDbContext ctx, TestClock clock)
            => new CustomerService(ctx, clock);

        /// <summary>
        /// Garante que customerId inválido (0 ou negativo) lança ArgumentOutOfRangeException.
        /// Protege contra chamadas incorretas logo no início.
        /// </summary>
        [Fact]
        public async Task Throws_When_CustomerId_Is_Invalid()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };
            var sut = CreateSut(ctx, clock);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.CanPurchase(0, 10));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.CanPurchase(-1, 10));
        }

        /// <summary>
        /// Garante que purchaseValue inválido (<= 0) lança ArgumentOutOfRangeException.
        /// Evita processamento com valores não permitidos.
        /// </summary>
        [Fact]
        public async Task Throws_When_PurchaseValue_Is_Invalid()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };
            var sut = CreateSut(ctx, clock);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.CanPurchase(1, 0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.CanPurchase(1, -5));
        }

        /// <summary>
        /// Garante que quando o cliente não existe, lança InvalidOperationException
        /// com a mensagem informando o Id.
        /// </summary>
        [Fact]
        public async Task Throws_When_Customer_Not_Found()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };
            var sut = CreateSut(ctx, clock);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CanPurchase(99, 50));
        }

        /// <summary>
        /// Impede compra se houver pedido nos últimos 30 dias (>= baseDate).
        /// Aqui simulamos 5 dias atrás, portanto deve retornar false.
        /// </summary>
        [Fact]
        public async Task Returns_False_When_Has_Order_In_Last_Month()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx, cfg =>
            {
                ctx.Orders.Add(new Order
                {
                    CustomerId = cfg.c3.Id,
                    OrderDate = clock.Now.AddDays(-5)
                });
            });

            var sut = CreateSut(ctx, clock);
            var result = await sut.CanPurchase(3, 50);
            Assert.False(result);
        }

        /// <summary>
        /// Borda do período: se o pedido ocorreu exatamente em baseDate (UtcNow - 1 mês),
        /// ainda deve contar como "recente" (>= baseDate) e bloquear a compra.
        /// </summary>
        [Fact]
        public async Task Returns_False_When_Order_Exactly_At_BaseDate()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc);
            var clock = new TestClock { Now = now };

            await SeedAsync(ctx, cfg =>
            {
                var baseDate = clock.Now.AddMonths(-1);
                ctx.Orders.Add(new Order
                {
                    CustomerId = cfg.c3.Id,
                    OrderDate = baseDate
                });
            });

            var sut = CreateSut(ctx, clock);
            var result = await sut.CanPurchase(3, 50);
            Assert.False(result);
        }

        /// <summary>
        /// Primeira compra: se valor for > 100, deve bloquear.
        /// </summary>
        [Fact]
        public async Task First_Purchase_Over_100_Returns_False()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 100.01m);
            Assert.False(result);
        }

        /// <summary>
        /// Primeira compra: valor exatamente 100 deve ser permitido em horário comercial.
        /// </summary>
        [Fact]
        public async Task First_Purchase_Exactly_100_During_Business_Hours_Returns_True()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 100m);
            Assert.True(result);
        }

        /// <summary>
        /// Primeira compra: valor mínimo positivo (ex.: 0,01) permitido em horário comercial.
        /// </summary>
        [Fact]
        public async Task First_Purchase_Min_Positive_Value_During_Business_Hours_Returns_True()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 0.01m);
            Assert.True(result);
        }

        /// <summary>
        /// Fora do horário: antes das 08:00 e após 18:00 deve bloquear, mesmo com demais regras ok.
        /// </summary>
        [Theory]
        [InlineData(7)]   // 07:00 (antes das 08)
        [InlineData(19)]  // 19:00 (depois das 18)
        public async Task Outside_Business_Hours_Returns_False(int hour)
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, hour, 0, 0, DateTimeKind.Utc) }; // quarta

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 50m);
            Assert.False(result);
        }

        /// <summary>
        /// Borda do horário: exatamente às 08:00 e 18:00 deve permitir (intervalo inclusivo).
        /// </summary>
        [Theory]
        [InlineData(8)]   // 08:00
        [InlineData(18)]  // 18:00
        public async Task Business_Hours_Boundaries_Return_True(int hour)
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            // Quarta-feira
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, hour, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 50m);
            Assert.True(result);
        }

        /// <summary>
        /// Fim de semana: sábado e domingo devem bloquear, mesmo no horário comercial.
        /// </summary>
        [Theory]
        [InlineData(DayOfWeek.Saturday)]
        [InlineData(DayOfWeek.Sunday)]
        public async Task Weekend_Returns_False(DayOfWeek dow)
        {
            // Base em um sábado real (16/08/2025)
            var baseDate = new DateTime(2025, 8, 16, 10, 0, 0, DateTimeKind.Utc);
            var date = baseDate;
            while (date.DayOfWeek != dow) date = date.AddDays(1);

            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = date };

            await SeedAsync(ctx);
            var sut = CreateSut(ctx, clock);

            var result = await sut.CanPurchase(1, 50m);
            Assert.False(result);
        }

        /// <summary>
        /// Cliente recorrente: com compra antiga (> 1 mês), sem compras recentes,
        /// horário comercial — compra liberada sem teto.
        /// </summary>
        [Fact]
        public async Task Returning_Customer_No_Recent_Orders_During_Business_Hours_Returns_True()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var clock = new TestClock { Now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc) };

            await SeedAsync(ctx, cfg =>
            {
                ctx.Orders.Add(new Order
                {
                    CustomerId = cfg.c2.Id,
                    OrderDate = clock.Now.AddMonths(-2)
                });
            });

            var sut = CreateSut(ctx, clock);
            var result = await sut.CanPurchase(2, 1000m);
            Assert.True(result);
        }

        /// <summary>
        /// Robustez: se existirem múltiplos pedidos recentes (redundância),
        /// ainda assim deve bloquear (qualquer contagem > 0).
        /// </summary>
        [Fact]
        public async Task Multiple_Recent_Orders_Still_Blocks()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc);
            var clock = new TestClock { Now = now };

            await SeedAsync(ctx, cfg =>
            {
                ctx.Orders.AddRange(
                    new Order { CustomerId = cfg.c3.Id, OrderDate = now.AddDays(-2) },
                    new Order { CustomerId = cfg.c3.Id, OrderDate = now.AddDays(-10) }
                );
            });

            var sut = CreateSut(ctx, clock);
            var result = await sut.CanPurchase(3, 50m);
            Assert.False(result);
        }

        /// <summary>
        /// Cliente que já comprou antes (qualquer compra no passado, mesmo antiga), 
        /// não possui limite de 100 nas próximas compras — apenas respeita mês/horário/dia.
        /// </summary>
        [Fact]
        public async Task Returning_Customer_Ignores_FirstPurchase_Cap()
        {
            using var ctx = BuildContext(Guid.NewGuid().ToString());
            var now = new DateTime(2025, 8, 20, 10, 0, 0, DateTimeKind.Utc);
            var clock = new TestClock { Now = now };

            await SeedAsync(ctx, cfg =>
            {
                ctx.Orders.Add(new Order
                {
                    CustomerId = cfg.c2.Id,
                    OrderDate = now.AddMonths(-13) // compra bem antiga
                });
            });

            var sut = CreateSut(ctx, clock);
            var result = await sut.CanPurchase(2, 9999m);
            Assert.True(result);
        }
    }
}
