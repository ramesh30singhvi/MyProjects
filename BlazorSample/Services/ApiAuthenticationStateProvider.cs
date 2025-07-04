using Blazored.LocalStorage;
using CellarPassAppAdmin.Client.Helper;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class SavedToken
    {
        public IEnumerable<Claim> Claims { get; set; }
        public UserDetailViewModel SavedUser { get; set; } = new UserDetailViewModel();
    }
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider, ICellarPassAuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IApiManager _apiManager;
        public event Action NotifyUICurrentMemberChange;

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage,
            IApiManager apiManager)
        {
            _localStorage = localStorage;
            _apiManager = apiManager;
        }

        public async void ChangeCurrentMember()
        {
            await GetAuthenticationStateAsync();

            if (NotifyUICurrentMemberChange != null)
            {
                NotifyUICurrentMemberChange();
            }

        }



        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var authState = new ClaimsPrincipal(new ClaimsIdentity());
            SavedToken savedToken = await GetTokenAsync();

            if (string.IsNullOrWhiteSpace(savedToken.SavedUser.Token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            else
            {
                var claims = JwtParserHelper.ParseClaimsFromJwt(savedToken.SavedUser.Token);
                savedToken.SavedUser.UserFullName = claims.Where(x => x.Type == "unique_name").Select(x => x.Value).FirstOrDefault();
                //savedToken.SavedUser.Smsverified = claims.Where(x => x.Type == "Smsverified").Select(x => x.Value).FirstOrDefault();
                authState = await SetUserClaims(savedToken);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authState)));
            }
            return new AuthenticationState(authState);
        }
        public async Task MarkUserAsAuthenticated(UserDetailViewModel user)
        {
            SavedToken st = await ParseToken(user);
            await MarkUserAsAuthenticated(st);
        }
        private async Task MarkUserAsAuthenticated(SavedToken savedToken)
        {
            var authState = Task.FromResult(new AuthenticationState(await SetUserClaims(savedToken)));
            await _localStorage.SetItemAsync("token", savedToken.SavedUser.Token);
            await _localStorage.SetItemAsync("tokenExpire", savedToken.SavedUser.ExpirationDate);
            NotifyAuthenticationStateChanged(authState);
        }

        public async Task MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("tokenExpire");
            await _localStorage.ClearAsync();
            NotifyAuthenticationStateChanged(authState);
        }

        private async Task<SavedToken> GetTokenAsync()
        {
            var savedToken = await _localStorage.GetItemAsStringAsync("token");
            var expireDate = await _localStorage.GetItemAsync<DateTime>("tokenExpire");
            var user = await _localStorage.GetItemAsync<UserDetailViewModel>("user");
            if (user != null)
            {
                return await ParseToken(user);
            }
            else
            {
                return await ParseToken(new UserDetailViewModel()
                {
                    Token = savedToken,
                    ExpirationDate = expireDate
                });
            }
        }

        private async Task<SavedToken> ParseToken(UserDetailViewModel user)
        {
            if (string.IsNullOrWhiteSpace(user.Token))
            {
                return new SavedToken();
            }
            var tokenExpired = IsTokenExpired(user.ExpirationDate);
            if (tokenExpired)
            {
                await MarkUserAsLoggedOut();
                return new SavedToken();
            }
            var claims = JwtParserHelper.ParseClaimsFromJwt(user.Token);
            await _localStorage.SetItemAsync("user", user);
            string userId = claims.Where(x => x.Type == "nameid").Select(x => x.Value).FirstOrDefault();
            string userFullName = claims.Where(x => x.Type == "unique_name").Select(x => x.Value).FirstOrDefault();
            //string userSmsverified = claims.Where(x => x.Type == "Smsverified").Select(x => x.Value).FirstOrDefault();
            return new SavedToken()
            {
                Claims = claims,
                SavedUser = new UserDetailViewModel()
                {
                    Id = Convert.ToInt32(userId),
                    Token = user.Token,
                    ExpirationDate = user.ExpirationDate,
                    UserFullName = userFullName
                }
            };
        }

        private bool IsTokenExpired(DateTime expireDate)
        {
            return expireDate < DateTime.UtcNow;
        }

        private async Task<ClaimsPrincipal> SetUserClaims(SavedToken savedToken)
        {
            //create a claims
            var claimUserName = new Claim(ClaimTypes.Name, savedToken.SavedUser.UserFullName);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, Convert.ToString(savedToken.SavedUser.Id));
            //var claiSmsverified = new Claim("Smsverified", savedToken.SavedUser.Smsverified);

            List<Member> members = await _localStorage.GetItemAsync<List<Member>>("members");
            Member currentMember = await _localStorage.GetItemAsync<Member>("currentMember");
            var claimMembers = new Claim("members", "");
            var claimCurrentMember = new Claim("currentMember", "");
            var currentRoles = new List<string>();

            var isSuperAdmin = Convert.ToBoolean(savedToken.Claims.Where(x => x.Type == "isSuperAdmin").Select(x => x.Value).FirstOrDefault());
            if (members != null && members.Count > 0)
            {
                string json = JsonConvert.SerializeObject(members);
                claimMembers = new Claim("members", json);
            }
            if (currentMember != null)
            {
                string currentMemberJson = JsonConvert.SerializeObject(currentMember);
                claimCurrentMember = new Claim("currentMember", currentMemberJson);
                if (isSuperAdmin)
                {
                    currentRoles.Add("System Administrator");
                }
                else if (currentMember.Roles != null)
                {
                    currentRoles = currentMember.Roles.Select(x => x.Name).ToList();
                }

            }
            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimUserName, claimNameIdentifier, claimMembers }, "apiauth");

            foreach (var role in currentRoles)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            //create claimsPrincipal
            var authenticatedUser = new ClaimsPrincipal(claimsIdentity);
            return authenticatedUser;

        }

        public async Task<Member> GetCurrentMemberAsync()
        {
            // Current Member needs to be fetched for the pages which are dependent on the Current Member and are directly redirected by the URL after log In
            for(int trial = 0; trial < 20; trial++)
            {
                Member currentMember = await _localStorage.GetItemAsync<Member>("currentMember");
                if (currentMember != null)
                    return currentMember;
            }
            
            return new Member();
        }
    }
}
