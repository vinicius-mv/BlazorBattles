using BlazorBattles.Shared;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Services
{
    public interface IUtilityService
    {
        Task<User> GetUser();
    }
}
