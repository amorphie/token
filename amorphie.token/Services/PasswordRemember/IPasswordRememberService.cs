using amorphie.token.core.Models.RememberPassword;

namespace amorphie.token;

public interface IPasswordRememberService
{
    /// <summary>
    /// control card or cards info using citizenShipNo
    /// </summary>
    /// <param name="citizenshipNo"></param>
    /// <returns>Service response bool</returns>
    Task<ServiceResponse<bool>> HasCardAsync(string citizenshipNo);

    /// <summary>
    /// Get ekyc settings and check if video call is available
    /// for remember password process
    /// </summary>
    /// <returns></returns>
    Task<ServiceResponse<GetEkycSettings>> VideoCallAvailableAsync();
}
