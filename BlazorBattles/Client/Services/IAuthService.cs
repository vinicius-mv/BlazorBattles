using BlazorBattles.Shared;
using System.Threading.Tasks;

namespace BlazorBattles.Client.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> LoginAsync(UserLogin requet);
        Task<ServiceResponse<int>> RegisterAsync(UserRegister request);
    }
}