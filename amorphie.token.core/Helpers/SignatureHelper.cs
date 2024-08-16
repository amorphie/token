
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using amorphie.token.core.Enums;
using amorphie.token.core.Models.Token;
using Jose;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace amorphie.token.core.Helpers
{
    public class SignatureHelper
    {
        /// <summary>
        /// Set X-JWS-Signature header property
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="configuration"></param>
        /// <param name="body">Message body</param>
        public static void SetXJwsSignatureHeader(HttpContext httpContext, IConfiguration configuration, object body)
        {
            var headerPropData = GetXJwsSignature(body, configuration);
            httpContext.Response.Headers.Add("X-JWS-Signature", headerPropData);
        }

        public static string GetXJwsSignature(object body, IConfiguration configuration)
        {
            //JWT header-payload-signature

            // Create JWS header
            var jwtHeader = new Dictionary<string, object>()
        {
            { "alg", "RS256" },
            { "typ", "JWT" }
        };

            // Create JWS payload
            //From Document:
            //Payload kısmında özel olarak oluşturulacak olan “body” claim alanına istek gövdesi (request body) verisinin SHA256 hash değeri karşılığı yazılmalıdır.
            var data = new Dictionary<string, object>()
        {
            {"iss", "https://apigw.bkm.com.tr"},
            {"exp",  ((DateTimeOffset)DateTime.UtcNow.AddMinutes(60)).ToUnixTimeSeconds()},
            {"iat", ((DateTimeOffset)DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeSeconds()},
            {"body", GetChecksumSHA256(body)}
        };

            // Load private key from file
            var key = LoadPrivateKeyFromVault(configuration);
            return Jose.JWT.Encode(payload: data, key: key, JwsAlgorithm.RS256, extraHeaders: jwtHeader);
        }

        private static RSA LoadPrivateKeyFromPemFile(string pemFilePath)
        {
            string pemContents;
            using (StreamReader reader = new StreamReader(pemFilePath))
            {
                pemContents = reader.ReadToEnd();
            }
            var key = RSA.Create();
            key.ImportFromPem(pemContents);
            return key;
        }

        /// <summary>
        /// Loads hhsprivate key from vault
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns>RSA Key</returns>
        private static RSA LoadPrivateKeyFromVault(IConfiguration configuration)
        {
            string pemContents = configuration["HHS_PrivateKey"];
            var key = RSA.Create();
            key.ImportFromPem(pemContents);
            return key;
        }

        /// <summary>
        /// Generates sha256 hash of body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private static string GetChecksumSHA256(string body)
        {
            // Initialize a SHA256 hash object.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(body));
                return Convert.ToHexString(bytes);
            }
        }

        /// <summary>
        /// Generates sha256 hash of body
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        private static string GetChecksumSHA256(object body)
        {
            // Initialize a SHA256 hash object.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                var jsonString = JsonSerializer.Serialize(
                    body,
                    options: new JsonSerializerOptions{
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    }
                );
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(jsonString));
                return Convert.ToHexString(bytes);
            }
        }

        public static (bool,OpenBankingError?) ValidateFraudSignature(string jws, string signingKey)
        {
            RSA rsa = RSA.Create();
 
            // Convert the public key string to byte array
            byte[] publicKeyBytes = Convert.FromBase64String(signingKey);
    
            // Import the public key bytes to the RSA object
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateLifetime = true
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(jws, validationParameters, out Microsoft.IdentityModel.Tokens.SecurityToken securityToken);
                var FirstLoginFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("FirstLoginFlag"))?.Value;
                var DeviceFirstLoginFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("DeviceFirstLoginFlag"))?.Value;
                var LastPasswordChangeFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("LastPasswordChangeFlag"))?.Value;
                var BlacklistFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("BlacklistFlag"))?.Value;
                var MalwareFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("MalwareFlag"))?.Value;
                var AnomalyFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("AnomalyFlag"))?.Value;
                var UnsafeAccountFlag = principal.Claims.FirstOrDefault(c => c.Type.Equals("UnsafeAccountFlag"))?.Value;

                if(!CheckFraudPropertyIsValid(FirstLoginFlag, typeof(ZmnAralik), true))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(DeviceFirstLoginFlag, typeof(ZmnAralik), true))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(LastPasswordChangeFlag, typeof(ZmnAralik), true))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(BlacklistFlag, typeof(VarYok), false))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(MalwareFlag, typeof(ZmnAralik), false))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(AnomalyFlag, typeof(VarYok), false))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                if(!CheckFraudPropertyIsValid(UnsafeAccountFlag, typeof(ZmnAralik), false))
                    return (false,new OpenBankingError(){
                        ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                        HttpCode = 403,
                        HttpMessage = "Forbidden",
                        MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                        MoreInformation = "PSU-Fraud-Check header is invalid."
                    });
                
                return (true,null);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenInvalidSignatureException ex)
            {
                return (false,new OpenBankingError(){
                    ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                    HttpCode = 403,
                    HttpMessage = "Forbidden",
                    MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check kayitli public key ile dogrulanamadi.",
                    MoreInformation = "PSU-Fraud-Check signature does not match locally computed signature."
                });
            }
            catch(SecurityTokenExpiredException ex)
            {
                return (false,new OpenBankingError(){
                    ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                    HttpCode = 403,
                    HttpMessage = "Forbidden",
                    MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check tarihi gecmistir.",
                    MoreInformation = "PSU-Fraud-Check expired."
                });
            }
            catch(SecurityTokenMalformedException ex)
            {
                return (false,new OpenBankingError(){
                    ErrorCode = "TR.OHVPS.Resource.InvalidSignature",
                    HttpCode = 403,
                    HttpMessage = "Forbidden",
                    MoreInformationTr = "YOS ten gelen istekteki PSU-Fraud-Check basligi gecersiz.",
                    MoreInformation = "PSU-Fraud-Check header is invalid."
                });
            }

        }

        private static bool CheckFraudPropertyIsValid(string? val, Type enumType , bool isRequired)
        {
            if(string.IsNullOrWhiteSpace(val) && !isRequired)
                return true;
            
            if(string.IsNullOrWhiteSpace(val))
                return false;

            if(!Int32.TryParse(val, out var propVal) || !Enum.IsDefined(enumType, propVal))
            {
                return false;
            }

            return true;
        }
        

        public static bool ValidateSignature(string jws, string body, string signingKey)
        {
            RSA rsa = RSA.Create();
 
            // Convert the public key string to byte array
            byte[] publicKeyBytes = Convert.FromBase64String(signingKey);
    
            // Import the public key bytes to the RSA object
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateLifetime = true
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(jws, validationParameters, out Microsoft.IdentityModel.Tokens.SecurityToken securityToken);
                var bodyClaim = principal.Claims.FirstOrDefault(c => c.Type.Equals("body"))?.Value;
                var hashedBody = GetChecksumSHA256(body);
                if(!bodyClaim!.Equals(hashedBody,StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return true;
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenValidationException ex)
            {
                return false;
            }
            catch(SecurityTokenMalformedException ex)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Generates sha256 hash of body and xrequestId
        /// </summary>
        /// <returns>xRequestId|body sha256 hash</returns>
        public static string GetChecksumForXRequestIdSHA256(string body, string xRequestId)
        {
            string concatenatedData = $"{xRequestId}|{JsonSerializer.Serialize(body)}";
            // Initialize a SHA256 hash object.
            using SHA256 sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(concatenatedData));
            // Convert the hash bytes to a hexadecimal string.
            return Convert.ToHexString(bytes);
        }

        
    }
}