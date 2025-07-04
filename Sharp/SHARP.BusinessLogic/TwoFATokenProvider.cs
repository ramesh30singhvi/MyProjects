using Microsoft.AspNetCore.Identity;
using SHARP.DAL;
using SHARP.DAL.Models;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic
{
    public class TwoFATokenProvider : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        public const string NAME = "TwoFA";

        private readonly IUnitOfWork _unitOfWork;

        public TwoFATokenProvider(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public async Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            /*if (await _unitOfWork.TwoFATokenRepository.ExistsAsync(t => t.User.Id == user.Id))
            {
                _unitOfWork.TwoFATokenRepository.Remove(user.Id);

                await _unitOfWork.SaveChangesAsync();
            }*/

            var twoFAToken = await _unitOfWork.TwoFATokenRepository.GetAsync(user.Id);

            var token = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");

            if (twoFAToken == null)
            {
                var entity = new TwoFAToken
                {
                    Id = user.Id,
                    Token = BCrypt.Net.BCrypt.HashPassword(token),
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.TwoFATokenRepository.Add(entity);
            }
            else
            {
                twoFAToken.Token = BCrypt.Net.BCrypt.HashPassword(token);
                twoFAToken.CreatedAt = DateTime.UtcNow;

                _unitOfWork.TwoFATokenRepository.Update(twoFAToken);
            }

            await _unitOfWork.SaveChangesAsync();

            return token;
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var issuedToken = await _unitOfWork.TwoFATokenRepository.GetAsync(user.Id);

            if (issuedToken == null)
            {
                return false;
            }

            var valid = BCrypt.Net.BCrypt.Verify(token, issuedToken.Token)
                && DateTime.UtcNow.AddMinutes(-5) <= issuedToken.CreatedAt;

            if (valid)
            {
                _unitOfWork.TwoFATokenRepository.Remove(issuedToken);

                await _unitOfWork.SaveChangesAsync();
            }

            return valid;
        }
    }
}
