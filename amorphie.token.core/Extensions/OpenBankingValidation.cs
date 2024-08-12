
using amorphie.token.core.Models.Token;

namespace amorphie.token.core.Extensions
{
    public static class OpenBankingValidation
    {
        public static (bool,OpenBankingError?) ValidateOpenBankingRequest(this OpenBankingTokenRequest request)
        {
            if(String.IsNullOrWhiteSpace(request.ConsentNo))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Consent - Consent field is mandatory.",
                    MoreInformationTr = "Geçersiz Rıza - Rıza alanı dolu olmalıdır."
                });
            }

            if(!Guid.TryParse(request.ConsentNo, out var _))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Consent - Consent field must be a valid guid.",
                    MoreInformationTr = "Geçersiz Rıza - Rıza alanı geçerli bir guid değeri olmalıdır."
                });
            }
            
            if(request!.ConsentType!.Equals("O") && request!.ConsentType!.Equals("H"))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Consent Type - Consent type field must be one of the following. (O/H)",
                    MoreInformationTr = "Geçersiz Rıza - Rıza tipi alanı şunlardan biri olmalıdır (O/H)."
                });
            }

            if(request!.AuthType!.Equals("yet_kod") && request!.ConsentType!.Equals("yenileme_belirteci"))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Auth Type - Auth type field must be one of the following. (yet_kod/yenileme_belirteci)",
                    MoreInformationTr = "Geçersiz Yetki Tipi - Yetki tipi alanı şunlardan biri olmalıdır (yet_kod/yenileme_belirteci)."
                });
            }

            if(request!.AuthType!.Equals("yet_kod") && string.IsNullOrWhiteSpace(request!.AuthCode))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Auth Code - Auth code field is mandatory.",
                    MoreInformationTr = "Geçersiz Yetki kodu - Yetki kodu alanı dolu olmalıdır."
                });
            }

            if(request!.AuthType!.Equals("yenileme_belirteci") && string.IsNullOrWhiteSpace(request!.RefreshToken))
            {
                return (false,new OpenBankingError{
                    HttpCode = 400,
                    HttpMessage = "Bad Request",
                    ErrorCode = "TR.OHVPS.Resource.InvalidFormat",
                    MoreInformation = "Invalid Refresh Token - Refresh token field is mandatory.",
                    MoreInformationTr = "Geçersiz Yenileme Belirteci - Yenileme belirteci alanı dolu olmalıdır."
                });
            }


            return (true,null);
            
        }
    }
}