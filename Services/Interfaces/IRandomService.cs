using ProvaPub.Models.Responses;

namespace ProvaPub.Services.Interfaces
{
    public interface IRandomService
    {
        Task<Response<int>> GetRandom();
    }
}
