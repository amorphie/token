
using amorphie.token.core.Dtos;

namespace amorphie.token.Services.User;

public interface IUserService
{
    public Task<ServiceResponse<LoginResponse>> Login(LoginRequest loginRequest);
    public Task<ServiceResponse<object>> CheckDevice(Guid userId, string clientId, string deviceId, Guid installationId);
    public Task<ServiceResponse<CheckDeviceWithoutUserResponseDto>> CheckDeviceWithoutUser(string clientId, string deviceId, Guid installationId);
    public Task<ServiceResponse<ActiveDeviceDto>> CheckDevice(string reference, string clientId);
    public Task<ServiceResponse> RemoveDevice(string reference, string clientId);
    public Task<ServiceResponse> SaveDevice(UserSaveMobileDeviceDto userSaveMobileDeviceDto);
    public Task<ServiceResponse<LoginResponse>> GetUserById(Guid userId);
    public Task<ServiceResponse<LoginResponse>> GetUserByReference(string reference);
    public Task<ServiceResponse> SaveUser(UserInfo userInfo);
    public Task<ServiceResponse<UserSecurityQuestionDto>> GetLastSecurityQuestion(Guid id);
    public Task<ServiceResponse<UserSecurityImageDto>> GetLastSecurityImage(Guid id);
    public Task<ServiceResponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions();
    public Task<ServiceResponse<IEnumerable<SecurityImageDto>>> GetSecurityImages();
    public Task<ServiceResponse<IEnumerable<UserClaimDto>>> GetUserClaims(Guid userId);
    public Task<ServiceResponse> MigrateSecurityQuestion(MigrateSecurityQuestionRequest migrateSecurityQuestionRequest);
    public Task<ServiceResponse> MigrateSecurityImage(MigrateSecurityImageRequest migrateSecurityImageRequest);
    public Task<ServiceResponse> MigrateSecurityImages(List<SecurityImageRequestDto> securityImageRequestDtos);
    public Task<ServiceResponse> MigrateSecurityQuestions(List<SecurityQuestionRequestDto> securityQuestionRequestDtos);
    public Task<ServiceResponse<GetPublicDeviceDto>> GetPublicDevice(string clientCode, string reference);
}
