using PlanningSystem.Interfaces.BLL;
using PlanningSystem.Models;
using PlanningSystem.Models.Models;
using PlanningSystem.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanningSystem.BLL
{
    public class ShiftLogic : BaseLogic, IShiftLogic
    {
        public ShiftLogic(BllFactory bllFactory) : base(bllFactory) { }

        public ResultObject<Shift> CreateShift(ShiftCreateRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                if (request.StartTime >= request.EndTime)
                    throw new Exception("Start time must be before end time");

                // Verify organization exists and user has access
                var organization = DalFactory.OrganizationRepository.GetById(request.OrganizationId);
                if (organization == null)
                    throw new Exception("Organization not found");

                var userMap = DalFactory.OrganizationRepository.GetUserMap(request.OrganizationId, userId);
                if (userMap == null)
                    throw new Exception("You do not have access to this organization");

                // Verify worker exists and is in the organization
                var worker = DalFactory.UserRepository.GetSingle(request.WorkerId);
                if (worker == null)
                    throw new Exception("Worker not found");

                var workerMap = DalFactory.OrganizationRepository.GetUserMap(request.OrganizationId, request.WorkerId);
                if (workerMap == null)
                    throw new Exception("Worker is not a member of this organization");

                // Check for overlapping shifts
                var existingShifts = DalFactory.ShiftRepository.GetByWorker(request.WorkerId);
                var overlapping = existingShifts.Any(s => 
                    (s.StartTime < request.EndTime && s.EndTime > request.StartTime) &&
                    s.Status != "missed");

                if (overlapping)
                    throw new Exception("Worker already has a shift scheduled during this time");

                var shift = new Shift
                {
                    Id = Guid.NewGuid(),
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Location = request.Location,
                    Status = request.Status ?? "scheduled",
                    OrganizationId = request.OrganizationId,
                    WorkerId = request.WorkerId
                };

                shift = DalFactory.ShiftRepository.Create(shift);
                return shift;
            });
        }

        public ResultObject<Shift> UpdateShift(Guid shiftId, ShiftUpdateRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                var shift = DalFactory.ShiftRepository.GetById(shiftId);
                if (shift == null)
                    throw new Exception("Shift not found");

                // Verify user has access to the organization
                var userMap = DalFactory.OrganizationRepository.GetUserMap(shift.OrganizationId, userId);
                if (userMap == null || (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager && userId != shift.WorkerId))
                    throw new Exception("You do not have permission to update this shift");

                // Update fields if provided
                if (request.StartTime.HasValue)
                    shift.StartTime = request.StartTime.Value;

                if (request.EndTime.HasValue)
                    shift.EndTime = request.EndTime.Value;

                if (request.StartTime.HasValue && request.EndTime.HasValue && shift.StartTime >= shift.EndTime)
                    throw new Exception("Start time must be before end time");

                if (!string.IsNullOrWhiteSpace(request.Location))
                    shift.Location = request.Location;

                if (request.WorkerId.HasValue)
                {
                    // Verify new worker exists and is in organization
                    var worker = DalFactory.UserRepository.GetSingle(request.WorkerId.Value);
                    if (worker == null)
                        throw new Exception("Worker not found");

                    var workerMap = DalFactory.OrganizationRepository.GetUserMap(shift.OrganizationId, request.WorkerId.Value);
                    if (workerMap == null)
                        throw new Exception("Worker is not a member of this organization");

                    shift.WorkerId = request.WorkerId.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (!IsValidStatus(request.Status))
                        throw new Exception("Invalid status. Must be: scheduled, finished, or missed");
                    shift.Status = request.Status;
                }

                // Check for overlapping shifts if time or worker changed
                if (request.StartTime.HasValue || request.EndTime.HasValue || request.WorkerId.HasValue)
                {
                    var workerId = request.WorkerId ?? shift.WorkerId;
                    var startTime = request.StartTime ?? shift.StartTime;
                    var endTime = request.EndTime ?? shift.EndTime;

                    var existingShifts = DalFactory.ShiftRepository.GetByWorker(workerId);
                    var overlapping = existingShifts.Any(s =>
                        s.Id != shiftId &&
                        (s.StartTime < endTime && s.EndTime > startTime) &&
                        s.Status != "missed");

                    if (overlapping)
                        throw new Exception("Worker already has a shift scheduled during this time");
                }

                DalFactory.ShiftRepository.Save(shift);
                return shift;
            });
        }

        public ResultObject<bool> DeleteShift(Guid shiftId, int userId)
        {
            return SafeExecute(() =>
            {
                var shift = DalFactory.ShiftRepository.GetById(shiftId);
                if (shift == null)
                    throw new Exception("Shift not found");

                // Verify user has permission (Admin, Manager, or the worker themselves)
                var userMap = DalFactory.OrganizationRepository.GetUserMap(shift.OrganizationId, userId);
                if (userMap == null || (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager && userId != shift.WorkerId))
                    throw new Exception("You do not have permission to delete this shift");

                DalFactory.ShiftRepository.Delete(shiftId);
                return true;
            });
        }

        public ResultObject<Shift> GetShift(Guid shiftId, int userId)
        {
            return SafeExecute(() =>
            {
                var shift = DalFactory.ShiftRepository.GetById(shiftId);
                if (shift == null)
                    throw new Exception("Shift not found");

                // Verify user has access to the organization
                var userMap = DalFactory.OrganizationRepository.GetUserMap(shift.OrganizationId, userId);
                if (userMap == null)
                    throw new Exception("You do not have access to this shift");

                return shift;
            });
        }

        public ResultObject<List<Shift>> GetShifts(ShiftFilterRequest filter, int userId)
        {
            return SafeExecute(() =>
            {
                List<Shift> shifts;

                if (filter == null)
                {
                    // Get all shifts for organizations the user belongs to
                    var userOrganizations = DalFactory.OrganizationRepository.GetByUserId(userId);
                    var organizationIds = userOrganizations.Select(o => o.Id).ToList();
                    
                    shifts = new List<Shift>();
                    foreach (var orgId in organizationIds)
                    {
                        var orgShifts = DalFactory.ShiftRepository.GetByOrganization(orgId);
                        shifts.AddRange(orgShifts);
                    }
                }
                else
                {
                    // If organization filter is specified, verify user has access
                    if (filter.OrganizationId.HasValue)
                    {
                        var userMap = DalFactory.OrganizationRepository.GetUserMap(filter.OrganizationId.Value, userId);
                        if (userMap == null)
                            throw new Exception("You do not have access to this organization");
                    }

                    shifts = DalFactory.ShiftRepository.GetFiltered(filter);
                }

                return shifts.OrderBy(s => s.StartTime).ToList();
            });
        }

        public ResultObject<Shift> UpdateShiftStatus(Guid shiftId, ShiftStatusUpdateRequest request, int userId)
        {
            return SafeExecute(() =>
            {
                var shift = DalFactory.ShiftRepository.GetById(shiftId);
                if (shift == null)
                    throw new Exception("Shift not found");

                // Verify user has permission (Admin, Manager, or the worker themselves)
                var userMap = DalFactory.OrganizationRepository.GetUserMap(shift.OrganizationId, userId);
                if (userMap == null || (userMap.Role != OrganizationRole.Admin && userMap.Role != OrganizationRole.Manager && userId != shift.WorkerId))
                    throw new Exception("You do not have permission to update this shift");

                if (!IsValidStatus(request.Status))
                    throw new Exception("Invalid status. Must be: scheduled, finished, or missed");

                shift.Status = request.Status;
                DalFactory.ShiftRepository.Save(shift);
                return shift;
            });
        }

        private bool IsValidStatus(string status)
        {
            return status == "scheduled" || status == "finished" || status == "missed";
        }
    }
}
