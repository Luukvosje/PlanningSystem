using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;

namespace PlanningSystem.Application.Interfaces;

public interface IUserApplicationService
{
    ResultObject<Domain.Entities.User> CreateUser(UserAddRequest request);
    ResultObject<UserAuthResponse> AuthenticateUser(UserAuthRequest request, string jwtKey);
}
