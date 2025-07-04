using AutoMapper;
using Microsoft.AspNetCore.Http;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.BusinessLogic.Extensions;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Constants;
using SHARP.Common.Enums;
using SHARP.Common.Filtration;
using SHARP.Common.Models;
using SHARP.DAL;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class MemoService : IMemoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppConfig _appConfig;

        public MemoService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            AppConfig appConfig)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _appConfig = appConfig;
        }

        public async Task<IReadOnlyCollection<MemoDto>> GetMemosAsync(MemoFilter filter)
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() && !filter.OrganizationIds.Any())
            {
                filter.OrganizationIds = userOrganizationIds.ToList();
            }

            IReadOnlyCollection<Memo> memos = await _unitOfWork.MemoRepository.GetMemosAsync(filter);

            return _mapper.Map<IReadOnlyCollection<MemoDto>>(memos);
        }

        public async Task<MemoDto> GetMemoDetailsAsync(int id)
        {
            Memo memo = await _unitOfWork.MemoRepository.GetMemoDetailsAsync(id);

            if (memo == null)
            {
                throw new NotFoundException("Memo is not found");
            }

            return _mapper.Map<MemoDto>(memo);
        }

        public async Task<MemoDto> AddMemoAsync(AddMemoDto addMemoDto)
        {
            var memo = _mapper.Map<Memo>(addMemoDto);

            memo.UserId = _userService.GetLoggedUserId();
            memo.CreatedDate = DateTime.UtcNow;

            ValidateMemos(memo);

            var userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() && 
               (memo.OrganizationMemos == null || !memo.OrganizationMemos.Any() || 
               !memo.OrganizationMemos.All(memoOrg => userOrganizationIds.Contains(memoOrg.OrganizationId))))
            {
                throw new Exception("You can't add memos for chosen organization.");
            }

            await _unitOfWork.MemoRepository.AddAsync(memo);

            await _unitOfWork.SaveChangesAsync();

            return await GetMemoDetailsAsync(memo.Id);
        }

        public async Task<MemoDto> EditMemoAsync(EditMemoDto editMemoDto)
        {
            var memo = await _unitOfWork.MemoRepository.FirstOrDefaultAsync(
                memo => memo.Id == editMemoDto.Id,
                memo => memo.OrganizationMemos);

            _mapper.Map(editMemoDto, memo);

            ValidateMemos(memo);

            var userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() &&
               (memo.OrganizationMemos != null &&
               !memo.OrganizationMemos.All(memoOrg => userOrganizationIds.Contains(memoOrg.OrganizationId))))
            {
                throw new Exception("You can't edit memos for chosen organization.");
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetMemoDetailsAsync(memo.Id);
        }

        public async Task<bool> DeleteMemoAsync(int id)
        {
            var memo = await _unitOfWork.MemoRepository.FirstOrDefaultAsync(
               memo => memo.Id == id,
               memo => memo.OrganizationMemos);

            ValidateMemos(memo, true);

            var userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            if (userOrganizationIds.Any() && memo.OrganizationMemos != null && !memo.OrganizationMemos.All(memoOrg => userOrganizationIds.Contains(memoOrg.OrganizationId)))
            {
                throw new Exception("You can't delete this memo.");
            }

            _unitOfWork.MemoRepository.Remove(id);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private void ValidateMemos(Memo memo, bool isDelete = false)
        {
            string[] roles = _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)?.Select(claim => claim.Value).ToArray();

            if (!roles.Contains(UserRoles.Reviewer.ToString()) && !roles.Contains(UserRoles.Admin.ToString()))
            {
                throw new Exception("You can't add or edit memos.");
            }

            if (!roles.Contains(UserRoles.Admin.ToString()) && memo.UserId != _userService.GetLoggedUserId())
            {
                throw new Exception("You can't edit or delete this memo.");
            }
        }
    }
}
