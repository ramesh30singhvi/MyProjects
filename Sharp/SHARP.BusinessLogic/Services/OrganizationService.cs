using AutoMapper;
using SHARP.BusinessLogic.DTO.Organization;
using SHARP.BusinessLogic.Interfaces;
using SHARP.DAL;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SHARP.DAL.Models;
using SHARP.BusinessLogic.DTO;
using SHARP.BusinessLogic.Extensions;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System;
using System.Security.Claims;
using SHARP.BusinessLogic.DTO.Form;
using SHARP.Common.Filtration;
using SHARP.BusinessLogic.Helpers;
using SHARP.Common.Filtration.Enums;
using SHARP.Common.Enums;
using SHARP.Common.Constants;
using SHARP.DAL.Models.QueryModels;
using SHARP.BusinessLogic.DTO.Dashboard;

namespace SHARP.BusinessLogic.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IUserService _userService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrganizationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<OrganizationDto>> GetAsync()
        {
            var dal = await _unitOfWork.OrganizationRepository.GetListAsync(
                orderBySelector: org => org.Name);

            var dto = _mapper.Map<List<OrganizationDto>>(dal);
            dto.Insert(0, new OrganizationDto
            {
                Name = "All",
                Unlimited = true
            });

            return dto;
        }

        public List<OrganizationDetailedDto> GetDetailed()
        {
            var organizations = _unitOfWork.OrganizationRepository
                .GetAll()
                .Include(organization => organization.Facilities)
                .Include(organization => organization.Recipients)
                .Include(organization => organization.PortalFeatures)
                .Select(organization => new OrganizationDetailedDto
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    OperatorName = organization.OperatorName,
                    OperatorEmail = organization.OperatorEmail, 
                    AttachPortalReport = organization.AttachPortalReport.GetValueOrDefault(),
                    FacilityCount = organization.Facilities.Count(),
                    Recipients = organization.Recipients.Select(recipient => new RecipientDto
                    { 
                        Id = recipient.Id,
                        Recipient = recipient.Recipient
                    }),
                    PortalFeatures = organization.PortalFeatures.Select(portalFeature => new
                        OrganizationPortalFeatureDto
                    {
                        Id = portalFeature.Id,
                        Name = portalFeature.PortalFeature.Name,
                        Available = portalFeature.Available,
                    })
                })
                .OrderBy(organization => organization.Name)
                .ToList();

            return organizations;
        }

        public async Task<IEnumerable<FormOrganizationDto>> GetOrganizationFormsAsync(FormFilter filter)
        {
            var orderBySelector = OrderByHelper.GetOrderBySelector<OrganizationFormFilterColumn, Expression<Func<FormOrganization, object>>>(
                    filter.OrderBy,
                    GetOrderBySelector);

            FormOrganization[] forms = await _unitOfWork.FormOrganizationRepository.GetOrganizationFormsAsync(filter, orderBySelector);

            return _mapper.Map<IEnumerable<FormOrganizationDto>>(forms);
        }

        public async Task<IReadOnlyCollection<FilterOption>> GetOrganizationFormFilterColumnSourceDataAsync(OrganizationFormFilterColumnSource<OrganizationFormFilterColumn> columnData)
        {
            if (columnData.Column == OrganizationFormFilterColumn.Undefined)
            {
                throw new InvalidOperationException("Column is invalid");
            }

            var columnSelector = GetColumnSelector(columnData.Column);

            var columnValues = await _unitOfWork.FormOrganizationRepository.GetDistinctColumnAsync(columnData, columnSelector);

            return _mapper.Map<IReadOnlyCollection<FilterOption>>(columnValues.OrderBy(i => i.Value));
        }

        private OrganizationDetailedDto MapToDetailedDto(Organization organization)
        {
            OrganizationDetailedDto detailedOrganization = new OrganizationDetailedDto();
            detailedOrganization.Id = organization.Id;
            detailedOrganization.Name = organization.Name;
            detailedOrganization.FacilityCount = organization.Facilities.Count();
            detailedOrganization.OperatorName = organization.OperatorName;
            detailedOrganization.OperatorEmail = organization.OperatorEmail;
            detailedOrganization.AttachPortalReport = organization.AttachPortalReport.GetValueOrDefault();
            return detailedOrganization;
        }

        public async Task<OrganizationDetailedDto> AddAsync(AddOrganizationDto addOrganization)
        {
            Organization newOrganization = _mapper.Map<Organization>(addOrganization);
            _unitOfWork.OrganizationRepository.Add(newOrganization);
            await _unitOfWork.SaveChangesAsync();
            var dbOrganization = _unitOfWork.OrganizationRepository.GetAll().Include(organization=>organization.Facilities).Where(organization => organization.Id == newOrganization.Id).SingleOrDefault();
            OrganizationDetailedDto detailedOrganization = null;
            if(dbOrganization != null)
            {
                detailedOrganization = MapToDetailedDto(dbOrganization);
            }

            return detailedOrganization;
        }

        public bool OrganizationExists(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            name = name.Trim().ToLower();
            return _unitOfWork.OrganizationRepository.Any(organization => organization.Name.ToLower() == name);
        }

        public bool OrganizationExists(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            name = name.Trim().ToLower();
            return _unitOfWork.OrganizationRepository.Any(organization => organization.Name.ToLower() == name && organization.Id != id);
        }
        
        public async Task<bool> EditOrganizationAsync(EditOrganizationDto editOrganization)
        {
            Organization organization = await _unitOfWork.OrganizationRepository.SingleAsync(editOrganization.Id);

            organization.Name = editOrganization.Name;
            organization.OperatorEmail = editOrganization.OperatorEmail;
            organization.OperatorName = editOrganization.OperatorName;
            organization.AttachPortalReport = editOrganization.AttachPortalReport;

            var toDelete = organization.Recipients.ToArray();
            _unitOfWork.RecipientRepository.RemoveRange(toDelete);

            organization.Recipients = editOrganization.Recipients
                .Select(recipient => new OrganizationRecipient
                {
                    Recipient = recipient,
                    OrganizationId = editOrganization.Id
                }).ToList();

            _unitOfWork.OrganizationRepository.Update(organization);

            await _unitOfWork.SaveChangesAsync();

            if (!organization.PortalFeatures.Any())
            {
                await AddPortalFeatures(organization.Id, editOrganization.PortalFeatures);
            }
            else
            {
                await EditPortalFeatures(organization, editOrganization.PortalFeatures);
            }
            return true;
        }

        private async Task EditPortalFeatures(Organization organization, IEnumerable<OrganizationPortalFeatureDto> portalFeatures)
        {
            foreach (var feature in portalFeatures)
            {
                if (organization.PortalFeatures.Any( f => f.Id == feature.Id ))
                {
                    var orgPortalFeature = organization.PortalFeatures.FirstOrDefault( f => f.Id == feature.Id );
                    orgPortalFeature.Available = feature.Available;
                    _unitOfWork.OrganizationPortalFeatureRepository.Update(orgPortalFeature);
                }
                else
                {
                    var newOrgPortalFeature = new OrganizationPortalFeature()
                    {
                        OrganizationId = organization.Id,
                        PortalFeatureId = feature.Id,
                        Available = feature.Available,
                    };
                    _unitOfWork.OrganizationPortalFeatureRepository.Add(newOrgPortalFeature);
                }

            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task AddPortalFeatures(int id, IEnumerable<OrganizationPortalFeatureDto> portalFeatures)
        {
           foreach(var feature in portalFeatures)
           {
                var newOrgPortalFeature = new OrganizationPortalFeature()
                {
                    OrganizationId = id,
                    PortalFeatureId = feature.Id,
                    Available = feature.Available,
                };
                _unitOfWork.OrganizationPortalFeatureRepository.Add(newOrgPortalFeature);
           }
           await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<OptionDto>> GetOrganizationOptionsAsync()
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            Expression<Func<Organization, bool>> predicate = null;

            if (userOrganizationIds.Any())
            {
                predicate = organization => userOrganizationIds.Contains(organization.Id);
            }

            IReadOnlyCollection<Organization> organizations = await _unitOfWork.OrganizationRepository.GetListAsync(
                predicate,
                organization => organization.Name,
                asNoTracking: true);

            return _mapper.Map<IReadOnlyCollection<OptionDto>>(organizations);
        }

        public async Task<bool> SetFormSettingAsync(FormSettingDto formSettingDto)
        {
            FormOrganization formOrganization = await _unitOfWork.FormOrganizationRepository.FirstOrDefaultAsync(
                formOrganization => formOrganization.Id == formSettingDto.Id,
                formOrganization => formOrganization.ScheduleSetting);

            _mapper.Map(formSettingDto, formOrganization);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        //For AutoTests
        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            var organization = await _unitOfWork.OrganizationRepository.GetAll()
                .Include(org => org.FormOrganizations)
                    .ThenInclude(formOrg => formOrg.Form)
                .FirstOrDefaultAsync(form => form.Id == id);

            if(organization == null)
            {
                throw new NotFoundException($"Organization with Id: {id} is not found.");
            }

            foreach (var formOrganization in organization.FormOrganizations)
            {
                _unitOfWork.FormOrganizationRepository.Remove(formOrganization);

                _unitOfWork.FormRepository.Remove(formOrganization.Form);
            }

            _unitOfWork.OrganizationRepository.Remove(organization);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private Expression<Func<FormOrganization, object>> GetOrderBySelector(OrganizationFormFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<OrganizationFormFilterColumn, Expression<Func<FormOrganization, object>>>
            {
                {
                    OrganizationFormFilterColumn.Name,
                    i => i.Form.Name
                },
                {
                    OrganizationFormFilterColumn.AuditType,
                    i => i.Form.AuditType.Name
                },
                {
                    OrganizationFormFilterColumn.SettingType,
                    i => i.SettingType
                },
                {
                    OrganizationFormFilterColumn.ScheduleSetting,
                    i => i.ScheduleSetting.ScheduleType
                },
                {
                    OrganizationFormFilterColumn.IsFormActive,
                    i => i.Form.IsActive
                }
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var selector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return selector;
        }

        private Expression<Func<FormOrganization, FilterOptionQueryModel>> GetColumnSelector(OrganizationFormFilterColumn columnName)
        {
            var columnSelectorMap = new Dictionary<OrganizationFormFilterColumn, Expression<Func<FormOrganization, FilterOptionQueryModel>>>
            {
                { OrganizationFormFilterColumn.Name, i => new FilterOptionQueryModel { Value = i.Form.Name } },
                { OrganizationFormFilterColumn.AuditType, i =>  new FilterOptionQueryModel { Id = i.Form.AuditTypeId, Value = i.Form.AuditType.Name } },
            };

            if (!columnSelectorMap.TryGetValue(columnName, out var columnSelector))
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), columnName, null);
            }

            return columnSelector;
        }

        public async Task<OptionDto> GetOrganizationAsync(int id)
        {
            var organization = await _unitOfWork.OrganizationRepository.GetAsync<int>(id);
            if (organization == null)
            {
                return null;
            }

            return _mapper.Map<OptionDto>(organization);
        }

        public async Task<OrganizationDetailedDto> GetDetailedAsync(int id)
        {
            var organization = await _unitOfWork.OrganizationRepository.GetOneAsync(id);
            return _mapper.Map<OrganizationDetailedDto>(organization);
        }
    }
}
