using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBattles.Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;

        public CustomAuthStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            AuthenticationState state;
            if (await _localStorageService.GetItemAsync<bool>("isAuthenticated"))
            {
                var identity = new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, "Vinicius") }, 
                    "test authenticaiton type");

                var user = new ClaimsPrincipal(identity);
                state = new AuthenticationState(user);
                
                NotifyAuthenticationStateChanged(Task.FromResult(state));

                return state;
            }
            state = new AuthenticationState(new ClaimsPrincipal()); // unauthenticated user
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }
    }
}
