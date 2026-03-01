using PlanningSystem.Application.Common;
using PlanningSystem.Application.DTOs.Requests;
using PlanningSystem.Application.Interfaces;
using PlanningSystem.Domain.Entities;
using PlanningSystem.Domain.Exceptions;

namespace PlanningSystem.Application.Services;

public class UserApplicationService : IUserApplicationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public UserApplicationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public ResultObject<User> CreateUser(UserAddRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
                throw new ValidationException("Missing 'Email' value");

            if (string.IsNullOrWhiteSpace(request?.Password))
                throw new ValidationException("Missing 'Password' value");

            var existingUser = _userRepository.GetByEmail(request.Email.Trim().ToLower());
            if (existingUser != null)
                throw new ValidationException("User already exists");

            var newUser = new User
            {
                Guid = request.Guid != default ? request.Guid : Guid.NewGuid(),
                Email = request.Email.Trim().ToLower(),
                Name = (request.Name ?? "").Trim(),
                Pwd = _passwordHasher.HashPassword(request.Password ?? "")
            };

            newUser = _userRepository.Create(newUser);
            _unitOfWork.Commit();

            return new ResultObject<User> { Success = true, Data = newUser };
        }
        catch (DomainException ex)
        {
            return new ResultObject<User> { Success = false, Message = ex.Message, Exception = ex };
        }
    }

    public ResultObject<UserAuthResponse> AuthenticateUser(UserAuthRequest request, string jwtKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
                throw new ValidationException("Email is required");

            if (string.IsNullOrWhiteSpace(request?.Password))
                throw new ValidationException("Password is required");

            var user = _userRepository.GetByEmail(request.Email.Trim().ToLower());
            if (user == null)
                throw new ValidationException("Invalid email or password");

            if (string.IsNullOrWhiteSpace(user.Pwd) || !_passwordHasher.VerifyPassword(request.Password, user.Pwd))
                throw new ValidationException("Invalid email or password");

            var token = _jwtTokenGenerator.GenerateToken(user, jwtKey);

            return new ResultObject<UserAuthResponse>
            {
                Success = true,
                Data = new UserAuthResponse
                {
                    Token = token,
                    User = new User { Id = user.Id, Guid = user.Guid, Name = user.Name, Email = user.Email }
                }
            };
        }
        catch (DomainException ex)
        {
            return new ResultObject<UserAuthResponse> { Success = false, Message = ex.Message, Exception = ex };
        }
    }
}
