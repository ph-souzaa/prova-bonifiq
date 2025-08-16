using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
	public class RandomService
	{   
        int seed;
        TestDbContext _ctx;
		public RandomService(TestDbContext ctx)
        {
        // está no program entao só precisa passar na dependencia aq
            _ctx = ctx;
            seed = Guid.NewGuid().GetHashCode();
        }
        public async Task<int> GetRandom()
		{
            var number =  new Random(seed).Next(100);
            _ctx.Numbers.Add(new RandomNumber() { Number = number });
            await _ctx.SaveChangesAsync();
			return number;
		}

	}
}
