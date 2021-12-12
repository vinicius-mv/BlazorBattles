using BlazorBattles.Shared;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorBattles.Client.Services
{
    public class BattleService : IBattleService
    {
        private readonly HttpClient _http;

        public BattleService(HttpClient http)
        {
            _http = http;
        }

        public BattleResult LastBattle { get; set; } = new BattleResult();
        public IList<BattleHistoryEntry> History { get; set; } = new List<BattleHistoryEntry>();

        public async Task<BattleResult> StartBattleAsync(int opponentId)
        {
            var result = await _http.PostAsJsonAsync("api/battles", opponentId);
            LastBattle = await result.Content.ReadFromJsonAsync<BattleResult>();

            return LastBattle;
        }

        public async Task GetHistoryAsync()
        {
            History = await _http.GetFromJsonAsync<BattleHistoryEntry[]>("api/user/history");
        }
    }
}
