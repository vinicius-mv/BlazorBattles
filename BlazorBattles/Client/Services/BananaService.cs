using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorBattles.Client.Services
{
    public class BananaService : IBananaService
    {
        private readonly HttpClient _http;

        public event Action OnChange;

        public int Bananas { get; set; }

        public BananaService(HttpClient http)
        {
            _http = http;
        }

        public void EatBananas(int amount)
        {
            Bananas -= amount;
            BananasChanged();
        }

        void BananasChanged() => OnChange.Invoke();

        public async Task AddBananas(int amount)
        {
            var result = await _http.PutAsJsonAsync<int>("api/user/addBananas", amount);
            Bananas = await result.Content.ReadFromJsonAsync<int>();    
            BananasChanged();
        }

        public async Task GetBananasAsync()
        {
            Bananas = await _http.GetFromJsonAsync<int>("api/user/getbananas");
            BananasChanged();
        }
    }
}
