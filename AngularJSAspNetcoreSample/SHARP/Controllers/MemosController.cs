using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.DTO.Memo;
using SHARP.BusinessLogic.Interfaces;
using SHARP.BusinessLogic.Services;
using SHARP.Common.Constants;
using SHARP.Common.Filtration;
using SHARP.Common.Models;
using SHARP.Filters;
using SHARP.ViewModels.Memo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemosController : ControllerBase
    {
        private readonly IMemoService _memoService;
        private readonly IUserService _userService;
        private readonly AppConfig _appConfig;

        private readonly IMapper _mapper;

        public MemosController(
            IMemoService memoService,
            IUserService userService,
            AppConfig appConfig,
            IMapper mapper)
        {
            _memoService = memoService;
            _mapper = mapper;
            _userService = userService;
            _appConfig = appConfig;
        }

        [Route("get")]
        [HttpPost]
        public async Task<IActionResult> GetMemos([FromBody] MemoFilterModel memoFilter)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var filter = _mapper.Map<MemoFilter>(memoFilter);

            IReadOnlyCollection<MemoDto> memosDto = await _memoService.GetMemosAsync(filter);

            var memos = _mapper.Map<IReadOnlyCollection<MemoModel>>(memosDto);

            return Ok(memos);
        }

        [HttpPost]
        [ValidateModel]
        [Authorize(Roles = "Admin, Reviewer")]
        public async Task<IActionResult> AddMemo([FromBody] AddMemoModel memoModel)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var addMemoDto = _mapper.Map<AddMemoDto>(memoModel);

            MemoDto memoDto = await _memoService.AddMemoAsync(addMemoDto);

            var memo = _mapper.Map<MemoModel>(memoDto);

            return Ok(memo);
        }

        [HttpPut]
        [ValidateModel]
        [Authorize(Roles = "Admin, Reviewer")]
        public async Task<IActionResult> EditMemo([FromBody] EditMemoModel memoModel)
        {
            _appConfig.Application[CommonConstants.USER_TIME_ZONE] = _userService.GetCurrentUserTimeZone();

            var editMemoDto = _mapper.Map<EditMemoDto>(memoModel);

            MemoDto memoDto = await _memoService.EditMemoAsync(editMemoDto);

            var memo = _mapper.Map<MemoModel>(memoDto);

            return Ok(memo);
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "Admin, Reviewer")]
        public async Task<IActionResult> DeleteMemo(int id)
        {
            bool result = await _memoService.DeleteMemoAsync(id);

            return Ok(result);
        }
    }
}
