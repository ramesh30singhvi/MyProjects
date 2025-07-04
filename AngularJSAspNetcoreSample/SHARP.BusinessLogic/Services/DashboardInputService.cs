using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Presentation;
using SHARP.BusinessLogic.DTO.Dashboard;
using SHARP.BusinessLogic.Interfaces;
using SHARP.Common.Filtration;
using SHARP.DAL;
using SHARP.DAL.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using User = SHARP.DAL.Models.User;

namespace SHARP.BusinessLogic.Services
{
	public class DashboardInputService : IDasbhoardInputService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;
        private readonly IOrganizationService _organizationService;

        public DashboardInputService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IAuditService auditService,
            IOrganizationService organizationService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _auditService = auditService;
            _organizationService = organizationService;
        }

        public async Task<IReadOnlyCollection<DashboardInputDto>> GetDashboardInputAsync(DashboardInputFilterDto dashboardInputFilter)
        {

            await this.InitDashboardInputForOrganization(dashboardInputFilter.organizationId);

            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();
            userOrganizationIds.Clear();
            userOrganizationIds.Add(dashboardInputFilter.organizationId);

            var organizations = await _unitOfWork
                .OrganizationRepository.GetFullAsync(userOrganizationIds);

            List<DashboardInputDto> dashboardInputDtos = _mapper.Map<List<DashboardInputDto>>(organizations);

            DateTime startDate = dashboardInputFilter.dateFrom;
            DateTime endDate = dashboardInputFilter.dateTo;

            List<DateTime> listDays = GetListDaysFromRange(startDate, endDate);

            foreach (DashboardInputDto dashboardInputDto in dashboardInputDtos)
            {
                List<DashboardInputSummary> dashboardInputSummaries = new List<DashboardInputSummary>();

                var UserOrganization = _unitOfWork.UserOrganizationRepository.Find(user => user.OrganizationId == dashboardInputDto.Id).ToList();
                foreach (UserOrganization userOrganization in UserOrganization)
                {

                    User user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(user => user.Id == userOrganization.UserId);

                    DashboardInputSummary dashboardInputSummary = new DashboardInputSummary();
                    dashboardInputSummary.Auditor = $"{user.FullName}";

                    List<DashboardInputSummaryShift> dashboardInputSummaryShifts = new List<DashboardInputSummaryShift>();

                    // 8AM-10AM;

                    var auditIds = new List<int>();

                    foreach (DateTime day in listDays)
                    {
                        DateTime startTime = new DateTime(day.Year, day.Month, day.Day, 8, 0, 0);
                        DateTime endTime = new DateTime(day.Year, day.Month, day.Day, 9, 59, 59);

                        var audits = await _auditService.GetAuditsByUserTimeAndShift(dashboardInputDto.Id, user.Id, startTime, endTime);

                        auditIds = auditIds.Concat(audits.ConvertAll(a => a.Id)).ToList();

                        var shift = dashboardInputSummaryShifts.Where(x => x.Name == "8AM-10AM").FirstOrDefault();

                        if (shift != null)
                        {
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].Total += audits.Count;
                            var listFormNames = dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames;
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames = listFormNames.Concat(audits.ConvertAll(a => a.FormVersion.Form.Name)).Distinct().ToList();
                        }
                        else
                        {
                            DashboardInputSummaryShift dashboardInputSummaryShift = new DashboardInputSummaryShift();
                            dashboardInputSummaryShift.Name = "8AM-10AM";
                            dashboardInputSummaryShift.Total = audits.Count;
                            dashboardInputSummaryShift.FormNames = audits.ConvertAll(a => a.FormVersion.Form.Name).Distinct().ToList();
                            dashboardInputSummaryShifts.Add(dashboardInputSummaryShift);
                        }
                    }

                    // 10AM-12PM;
                    foreach (DateTime day in listDays)
                    {
                        DateTime startTime = new DateTime(day.Year, day.Month, day.Day, 10, 0, 0);
                        DateTime endTime = new DateTime(day.Year, day.Month, day.Day, 11, 59, 59);

                        var audits = await _auditService.GetAuditsByUserTimeAndShift(dashboardInputDto.Id, user.Id, startTime, endTime);

                        auditIds = auditIds.Concat(audits.ConvertAll(a => a.Id)).ToList();

                        var shift = dashboardInputSummaryShifts.Where(x => x.Name == "10AM-12PM").FirstOrDefault();

                        if (shift != null)
                        {
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].Total += audits.Count;
                            var listFormNames = dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames;
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames = listFormNames.Concat(audits.ConvertAll(a => a.FormVersion.Form.Name)).Distinct().ToList();
                        }
                        else
                        {
                            DashboardInputSummaryShift dashboardInputSummaryShift = new DashboardInputSummaryShift();
                            dashboardInputSummaryShift.Name = "10AM-12PM";
                            dashboardInputSummaryShift.Total = audits.Count;
                            dashboardInputSummaryShift.FormNames = audits.ConvertAll(a => a.FormVersion.Form.Name).Distinct().ToList();
                            dashboardInputSummaryShifts.Add(dashboardInputSummaryShift);
                        }
                    }

                    // 1PM-3PM;
                    foreach (DateTime day in listDays)
                    {
                        DateTime startTime = new DateTime(day.Year, day.Month, day.Day, 13, 0, 0);
                        DateTime endTime = new DateTime(day.Year, day.Month, day.Day, 14, 59, 59);

                        var audits = await _auditService.GetAuditsByUserTimeAndShift(dashboardInputDto.Id, user.Id, startTime, endTime);

                        auditIds = auditIds.Concat(audits.ConvertAll(a => a.Id)).ToList();

                        var shift = dashboardInputSummaryShifts.Where(x => x.Name == "1PM-3PM").FirstOrDefault();

                        if (shift != null)
                        {
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].Total += audits.Count;
                            var listFormNames = dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames;
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames = listFormNames.Concat(audits.ConvertAll(a => a.FormVersion.Form.Name)).Distinct().ToList();
                        }
                        else
                        {
                            DashboardInputSummaryShift dashboardInputSummaryShift = new DashboardInputSummaryShift();
                            dashboardInputSummaryShift.Name = "1PM-3PM";
                            dashboardInputSummaryShift.Total = audits.Count;
                            dashboardInputSummaryShift.FormNames = audits.ConvertAll(a => a.FormVersion.Form.Name).Distinct().ToList();
                            dashboardInputSummaryShifts.Add(dashboardInputSummaryShift);
                        }
                    }

                    // 3PM-5PM;
                    foreach (DateTime day in listDays)
                    {
                        DateTime startTime = new DateTime(day.Year, day.Month, day.Day, 15, 0, 0);
                        DateTime endTime = new DateTime(day.Year, day.Month, day.Day, 16, 59, 59);

                        var audits = await _auditService.GetAuditsByUserTimeAndShift(dashboardInputDto.Id, user.Id, startTime, endTime);

                        auditIds = auditIds.Concat(audits.ConvertAll(a => a.Id)).ToList();

                        var shift = dashboardInputSummaryShifts.Where(x => x.Name == "3PM-5PM").FirstOrDefault();

                        if (shift != null)
                        {
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].Total += audits.Count;
                            var listFormNames = dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames;
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames = listFormNames.Concat(audits.ConvertAll(a => a.FormVersion.Form.Name)).Distinct().ToList();
                        }
                        else
                        {
                            DashboardInputSummaryShift dashboardInputSummaryShift = new DashboardInputSummaryShift();
                            dashboardInputSummaryShift.Name = "3PM-5PM";
                            dashboardInputSummaryShift.Total = audits.Count;
                            dashboardInputSummaryShift.FormNames = audits.ConvertAll(a => a.FormVersion.Form.Name).Distinct().ToList();
                            dashboardInputSummaryShifts.Add(dashboardInputSummaryShift);
                        }
                    }

                    // OVERTIME;
                    foreach (DateTime day in listDays)
                    {
                        DateTime startTimeOvertime = new DateTime(day.Year, day.Month, day.Day, 17, 0, 0);
                        DateTime endTimeOvertime = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);

                        var auditsOvertime = await _auditService.GetAuditsByUserTimeAndShift(dashboardInputDto.Id, user.Id, startTimeOvertime, endTimeOvertime);

                        auditsOvertime = auditsOvertime.FindAll(a => auditIds.Contains(a.Id) == false);

                        var shift = dashboardInputSummaryShifts.Where(x => x.Name == "OVERTIME").FirstOrDefault();

                        if (shift != null)
                        {
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].Total += auditsOvertime.Count;
                            var listFormNames = dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames;
                            dashboardInputSummaryShifts[dashboardInputSummaryShifts.IndexOf(shift)].FormNames = listFormNames.Concat(auditsOvertime.ConvertAll(a => a.FormVersion.Form.Name)).Distinct().ToList();
                        }
                        else
                        {
                            DashboardInputSummaryShift dashboardInputSummaryShiftOvertime = new DashboardInputSummaryShift();
                            dashboardInputSummaryShiftOvertime.Name = "OVERTIME";
                            dashboardInputSummaryShiftOvertime.Total = auditsOvertime.Count;
                            dashboardInputSummaryShiftOvertime.FormNames = auditsOvertime.ConvertAll(a => a.FormVersion.Form.Name).Distinct().ToList();

                            dashboardInputSummaryShifts.Add(dashboardInputSummaryShiftOvertime);
                        }

                    }

                    dashboardInputSummary.DashboardInputSummaryShift = dashboardInputSummaryShifts.ToArray();

                    dashboardInputSummaries.Add(dashboardInputSummary);
                }

                dashboardInputDto.DashboardInputSummaries = dashboardInputSummaries.ToArray();
            }
            

            return dashboardInputDtos;
        }

        private List<DateTime> GetListDaysFromRange(DateTime from, DateTime to)
        {
            List<DateTime> days = new List<DateTime>();

            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                days.Add(day);

            if (days.Count == 0)
                days.Add(DateTime.Today);

            return days;
        }

        public async Task<IReadOnlyCollection<DashboardInputSummary>> GetDashboardInputAuditSummary()
        {
            ICollection<int> userOrganizationIds = await _userService.GetUserOrganizationIdsAsync();

            var organizations = await _unitOfWork
                .OrganizationRepository.GetFullAsync(userOrganizationIds);

            List<DashboardInputSummary> dashboardInputSummaries = new List<DashboardInputSummary>();

            return dashboardInputSummaries;
        }


        public async Task SaveDashboardInputValues(SaveDashboardInputValuesDto[] saveDashboardInputValuesDtos)
        {
            foreach(SaveDashboardInputValuesDto saveDashboardInputValueDto in saveDashboardInputValuesDtos)
            {
                if (saveDashboardInputValueDto.Id==0)
                {
                    DashboardInputValues newDashboardInputValue = _mapper.Map<DashboardInputValues>(saveDashboardInputValueDto);
                    _unitOfWork.DashboardInputValuesRepository.Add(newDashboardInputValue);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    DashboardInputValues dashboardInputValue = _unitOfWork.DashboardInputValuesRepository.Find(div => div.Id == saveDashboardInputValueDto.Id).Single();
                    dashboardInputValue.Value = saveDashboardInputValueDto.Value;
                    _unitOfWork.DashboardInputValuesRepository.Update(dashboardInputValue);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        async Task<DashboardInputDto> IDasbhoardInputService.AddElementToDashboardInput(AddElementDto addElementDto)
        {
            DashboardInputElement dashboardInputElement = _mapper.Map<DashboardInputElement>(addElementDto);
            _unitOfWork.DashboardInputElementRepository.Add(dashboardInputElement);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(addElementDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.AddGroupToDashboardInput(AddGroupDto addGroupDto)
        {
            DashboardInputGroups dashboardInputGroups = _mapper.Map<DashboardInputGroups>(addGroupDto);
            _unitOfWork.DashboardInputGroupsRepository.Add(dashboardInputGroups);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(addGroupDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.AddTableToDashboardInput(AddTableDto addTableDto)
        {
            DashboardInputTable dashboardInputTable = _mapper.Map<DashboardInputTable>(addTableDto);
            _unitOfWork.DashboardInputTableRepository.Add(dashboardInputTable);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(dashboardInputTable.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.DeleTableToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {
            _unitOfWork.DashboardInputTableRepository.Remove(editDashboardInputDto.Id);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.DeleteElementToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {
            _unitOfWork.DashboardInputElementRepository.Remove(editDashboardInputDto.Id);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization); 
        }

        async Task<DashboardInputDto> IDasbhoardInputService.DeleteGroupToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {
            _unitOfWork.DashboardInputGroupsRepository.Remove(editDashboardInputDto.Id);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.EditElementToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {

            DashboardInputElement dashboardInputElement = await _unitOfWork.DashboardInputElementRepository.FirstOrDefaultAsync(element => element.Id == editDashboardInputDto.Id);
            dashboardInputElement.Name = editDashboardInputDto.Name;
            dashboardInputElement.Keyword = editDashboardInputDto.Keyword;
            dashboardInputElement.FormId = editDashboardInputDto.FormId;
            _unitOfWork.DashboardInputElementRepository.Update(dashboardInputElement);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.EditGroupToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {
            DashboardInputGroups dashboardInputGroups = await _unitOfWork.DashboardInputGroupsRepository.FirstOrDefaultAsync(group => group.Id == editDashboardInputDto.Id);
            dashboardInputGroups.Name = editDashboardInputDto.Name;
            _unitOfWork.DashboardInputGroupsRepository.Update(dashboardInputGroups);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);
        }

        async Task<DashboardInputDto> IDasbhoardInputService.EditTableToDashboardInput(EditDashboardInputDto editDashboardInputDto)
        {
            DashboardInputTable dashboardInputTable = await _unitOfWork.DashboardInputTableRepository.FirstOrDefaultAsync(table => table.Id == editDashboardInputDto.Id);
            dashboardInputTable.Name = editDashboardInputDto.Name;
            _unitOfWork.DashboardInputTableRepository.Update(dashboardInputTable);
            await _unitOfWork.SaveChangesAsync();
            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(editDashboardInputDto.OrganizationId);


            return _mapper.Map<DashboardInputDto>(organization);

        }

        async Task<int> IDasbhoardInputService.FillElementsWithData(int organizationId, DashboardInputElement dashboardInputElement, Dictionary<string, int> pairs)
        {
            int updateCount = 0;

            var organization = await _unitOfWork
                .OrganizationRepository.GetOneAsync(organizationId);

            foreach (var facilty in organization.Facilities )
            {
                if (pairs.ContainsKey(facilty.Name))
                {
                    Console.WriteLine(pairs[facilty.Name]);
                    var saveDashboardInputValueDto = new SaveDashboardInputValuesDto();
                    saveDashboardInputValueDto.ElementId = dashboardInputElement.Id;
                    saveDashboardInputValueDto.Value = pairs[facilty.Name];
                    saveDashboardInputValueDto.Date = DateTime.Today;
                    saveDashboardInputValueDto.FacilityId = facilty.Id;
                    DashboardInputValues newDashboardInputValue = _mapper.Map<DashboardInputValues>(saveDashboardInputValueDto);
                    _unitOfWork.DashboardInputValuesRepository.Add(newDashboardInputValue);
                    await _unitOfWork.SaveChangesAsync();
                    updateCount++;
                }
            }

            return updateCount;
        }

        
        async Task<DashboardInputElement> IDasbhoardInputService.GetDashboardInputElement(int elementId)
        {
            return await _unitOfWork.DashboardInputElementRepository.FirstOrDefaultAsync(element => element.Id == elementId);
        }

        public async Task InitDashboardInputForOrganization(int organizationId)
        {
            List<string> tableNames = new List<string>() { "Daily", "Weekly", "Monthly" };

            foreach (var tableName in tableNames)
            {
                DashboardInputTable dit = await _unitOfWork.DashboardInputTableRepository.FirstOrDefaultAsync(table => table.OrganizationId == organizationId && table.Name == tableName);
                if (dit == null)
                {
                    dit = new DashboardInputTable();
                    dit.OrganizationId = organizationId;
                    dit.Name = tableName;
                    dit = _unitOfWork.DashboardInputTableRepository.Add(dit);
                    await _unitOfWork.SaveChangesAsync();
                }

                List<string> groupNames = new List<string>();

                switch (dit.Name)
                {
                    case "Daily":
                        groupNames = new List<string>() { "Dailys", "24 Hour Keyword and Supplementals", "PDPM" };
                        break;
                    case "Weekly":
                        groupNames = new List<string>() { "TUE", "BI-Weekly", "WED", "Bi-Weekly", "THUR", "FRI" };
                        break;
                    case "Monthly":
                        groupNames = new List<string>() { "1ST", "2ND", "3RD", "4TH" };
                        break;
                }

                foreach (var groupName in groupNames)
                {
                    DashboardInputGroups dig = await _unitOfWork.DashboardInputGroupsRepository.FirstOrDefaultAsync(dig => dig.TableId == dit.Id && dig.Name == groupName);
                    if (dig == null)
                    {
                        dig = new DashboardInputGroups();
                        dig.TableId = dit.Id;
                        dig.Name = groupName;
                        dig = _unitOfWork.DashboardInputGroupsRepository.Add(dig);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    List<string> elemementNames = new List<string>();

                    switch (dit.Name)
                    {
                        case "Daily":

                            switch (dig.Name)
                            {
                                case "Dailys":
                                    elemementNames = new List<string>() { "AA", "RV", "DH", "DC", "DE", "LAB" };
                                    break;
                                case "24 Hour Keyword and Supplementals":
                                    elemementNames = new List<string>() { "24KY", "FL", "BH", "WD", "AB", "REF", "CIC", "HOS", "PSY", "ABT", "AD", "FC", "SKILLED", "MAR" };
                                    break;
                                case "PDPM":
                                    elemementNames = new List<string>() { "5th" };
                                    break;
                            }

                            break;
                        case "Weekly":
                            switch (dig.Name)
                            {
                                case "TUE":
                                    elemementNames = new List<string>() { "RESP" };
                                    break;
                                case "BI-Weekly":
                                    elemementNames = new List<string>() { "MV", "CBO" };
                                    break;
                                case "WED":
                                    elemementNames = new List<string>() { "IV" };
                                    break;
                                case "Bi-Weekly":
                                    elemementNames = new List<string>() { "TRACH" };
                                    break;
                                case "THUR":
                                    elemementNames = new List<string>() { "WND" };
                                    break;
                                case "FRI":
                                    elemementNames = new List<string>() { "ADV", "ABT TR", "Fall TR", "WND TR", "LAB", "Trach" };
                                    break;

                            }
                            break;
                        case "Monthly":
                            switch (dig.Name)
                            {
                                case "1ST":
                                    elemementNames = new List<string>() { "FC", "HEMO", "PSY" };
                                    break;
                                case "2ND":
                                    elemementNames = new List<string>() { "E/W", "PSY", "Smoking", "SideR" };
                                    break;
                                case "3RD":
                                    elemementNames = new List<string>() { "Coumadin", "PSY" };
                                    break;
                                case "4TH":
                                    elemementNames = new List<string>() { "IMMU", "INS", "TF", "WTL", "PSY", "AD", "AC" };
                                    break;

                            }
                            break;
                    }

                    foreach (var elementName in elemementNames)
                    {
                        DashboardInputElement die = await _unitOfWork.DashboardInputElementRepository.FirstOrDefaultAsync(die => die.GroupId == dig.Id && die.Name == elementName);

                        if (die == null)
                        {
                            die = new DashboardInputElement();
                            die.GroupId = dig.Id;
                            die.Name = elementName;
                            die = _unitOfWork.DashboardInputElementRepository.Add(die);
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

            }

        }
    }
}

