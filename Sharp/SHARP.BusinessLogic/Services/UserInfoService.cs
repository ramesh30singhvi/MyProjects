using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Services
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserInfoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public User GetUserByAspnetUserID(string aspnetUserId)
        {
            return this._unitOfWork.UserRepository.GetAll().Where(a => a.UserId == aspnetUserId).SingleOrDefault();
        }
    }
}
