
using amorphie.token.core.Models.Consent;
using amorphie.token.core.Models.Profile;

namespace amorphie.token.Services.Token;

public interface ITokenService
{
    public Task<ServiceResponse<TokenResponse>> GenerateToken(GenerateTokenRequest tokenRequest);
    public Task<ServiceResponse<TokenResponse>> GenerateOpenBankingToken(GenerateTokenRequest tokenRequest,ConsentResponse consent);
    public Task<ServiceResponse<TokenResponse>> GenerateTokenWithPassword(GenerateTokenRequest tokenRequest);
    public Task<ServiceResponse<TokenResponse>> GenerateTokenWithRefreshToken(GenerateTokenRequest tokenRequest);
    public Task<ServiceResponse<TokenResponse>> GenerateTokenWithClientCredentials(GenerateTokenRequest tokenRequest);
    public Task<ServiceResponse<TokenResponse>> GenerateTokenWithPasswordFromWorkflow(GenerateTokenRequest tokenRequest, ClientResponse client, LoginResponse user, SimpleProfileResponse? profile, string deviceId);
}
