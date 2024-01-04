
using amorphie.token.core.Models.Profile;
using Refit;

namespace amorphie.token.Services.Profile
{
    public class ProfileService : ServiceBase, IProfileService
    {
        private readonly IProfile _profile;
        private readonly ISimpleProfile _simpleProfile;
        private ServiceResponse<ProfileResponse>? _profileResponse;
        private ServiceResponse<SimpleProfileResponse>? _simpleProfileResponse;
        public ProfileService(ILogger<ProfileService> logger, IConfiguration configuration, IProfile profile, ISimpleProfile simpleProfile) : base(logger, configuration)
        {
            _profile = profile;
            _simpleProfile = simpleProfile;
            _profileResponse = null;
            _simpleProfileResponse = null;
        }

        public async Task<ServiceResponse<ProfileResponse>> GetCustomerProfile(string reference)
        {
            if (_profileResponse != null)
                return _profileResponse;

            var result = new ServiceResponse<ProfileResponse>();
            try
            {
                var apiResponse = await _profile.GetProfile(reference, Configuration["ProfileUser"]!, Configuration["ProfileChannel"]!, Configuration["ProfileBranch"]!);

                result.Response = apiResponse;
                result.StatusCode = 200;
                _profileResponse = result;
            }
            catch (ApiException ex)
            {
                result.StatusCode = (int)ex.StatusCode;
                result.Detail = ex.ToString();
            }
            catch (Exception ex)
            {
                result.StatusCode = 500;
                result.Detail = ex.ToString();
            }


            return result;
        }

        public async Task<ServiceResponse<SimpleProfileResponse>> GetCustomerSimpleProfile(string reference)
        {
            if (_simpleProfileResponse != null)
                return _simpleProfileResponse;

            var result = new ServiceResponse<SimpleProfileResponse>();
            try
            {
                var apiResponse = await _simpleProfile.GetProfile(reference, Configuration["SimpleProfilePassword"]!);

                result.Response = apiResponse;
                result.StatusCode = 200;
                _simpleProfileResponse = result;
            }
            catch (ApiException ex)
            {
                result.StatusCode = (int)ex.StatusCode;
                result.Detail = ex.ToString();
            }
            catch (Exception ex)
            {
                result.StatusCode = 500;
                result.Detail = ex.ToString();
            }


            return result;
        }
    }
}