using BlazorBattles.Shared;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Data
{
    public interface IAuthRepository
    {
        Task<ServiceResponse<int>> RegisterAsync(User user, string password, int startUnitId);
        Task<ServiceResponse<string>> LoginAsync(string email, string password);
        Task<bool> UserExistsAsync(string email);
    }
}
