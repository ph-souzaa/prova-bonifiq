using ProvaPub.Services.Interfaces;

namespace ProvaPub.Services
{
    public sealed class SystemClockService : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
