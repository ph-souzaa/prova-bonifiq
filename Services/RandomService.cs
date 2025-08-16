using ProvaPub.Models;
using ProvaPub.Models.Responses;
using ProvaPub.Repository;
using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services
{
	public class RandomService : IRandomService
    {   
        int seed;
        TestDbContext _ctx;
		public RandomService(TestDbContext ctx)
        {
            _ctx = ctx;
            seed = Guid.NewGuid().GetHashCode();
        }
        public async Task<Response<int>> GetRandom()
		{
            var number =  new Random(seed).Next(100);
            _ctx.Numbers.Add(new RandomNumber() { Number = number });
            await _ctx.SaveChangesAsync();

            return Response<int>.Ok(number, "Número gerado com sucesso");
        }

	}
}
