using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using amorphie.token.core.Models.Consent;
using amorphie.token.core.Models.Profile;
using amorphie.token.Services.TransactionHandler;

namespace amorphie.token.Services.ClaimHandler
{
    public class ClaimHandlerService : ServiceBase, IClaimHandlerService
    {
        private readonly ITransactionService _transactionService;
        private readonly ITagService _tagService;

        private LoginResponse? _user;
        private ConsentResponse? _consent;
        private SimpleProfileResponse? _profile;
        private core.Models.Collection.User _collectionUser;

        //Using For Tag Service Query Params
        private string? _queryStringForTag;
        public ClaimHandlerService(
            ILogger<ClaimHandlerService> logger, IConfiguration configuration,
            ITransactionService transactionService, ITagService tagService)
            : base(logger, configuration)
        {
            _transactionService = transactionService;
            _tagService = tagService;

        }

        public async Task<string?> GetClaimDetail(string claimPathRaw)
        {
            string[] claimPath = claimPathRaw.Split(".");
            if(claimPath.First().Equals("NBSP"))
            {
                return " ";
            }
            if(claimPath.First().Equals("transaction"))
            {
                if (_transactionService == null)
                    return null;
                Type t = _transactionService!.GetType();

                var propValue = GetPropertyValue(_transactionService, string.Join('.', claimPath.ToList().Skip(1)));

                if (propValue != null)
                    return propValue!.ToString()!;
                else
                    return null;
            }
            if (claimPath.First().Equals("tag"))
            {
                try
                {
                    var domain = claimPath[1];
                    var entity = claimPath[2];
                    var tagName = claimPath[3];
                    var fieldName = claimPath[4];

                    var tagDataResponse = await _tagService.GetTagInfo(domain, entity, tagName, _queryStringForTag!);
                    if (tagDataResponse.StatusCode != 200)
                        return null;
                    var tagData = tagDataResponse.Response;

                    return tagData![fieldName]?.ToString() ?? null;

                }
                catch (Exception ex)
                {
                    Logger.LogError("Get Tag Info :" + ex.ToString());
                }
            }

            if (claimPath.First().Equals("user"))
            {
                if (_user == null)
                    return null;

                Type t = _user!.GetType();

                var property = t.GetProperties().FirstOrDefault(p => p.Name.ToLower(new CultureInfo("en-US",false)) == claimPath[1]);
                if (property == null)
                    return null;
                if (property!.GetValue(_user!) == null)
                    return null;

                return property!.GetValue(_user!)!.ToString()!;
            }

            if (claimPath.First().Equals("openbanking"))
            {
                if (_consent == null)
                    return null;

                Type t = _consent!.GetType();

                var property = t.GetProperties().FirstOrDefault(p => p.Name.ToLower() == claimPath[2]);
                if (property == null)
                    return null;
                if (property.GetValue(_consent) == null)
                    return null;

                return property!.GetValue(_consent)!.ToString()!;
            }

            if (claimPath.First().Equals("profile"))
            {
                if (_profile == null)
                    return null;

                Type t = _profile!.GetType();

                var propValue = GetPropertyValue(_profile, string.Join('.', claimPath.ToList().Skip(1)));
                
                if (propValue != null)
                    return propValue!.ToString()!;
                else
                    return null;

            }

            if (claimPath.First().Equals("collection"))
            {
                if (_collectionUser == null)
                    return null;

                Type t = _collectionUser!.GetType();

                var propValue = GetPropertyValue(_collectionUser, string.Join('.', claimPath.ToList().Skip(1)));
                
                if (propValue != null)
                    return propValue!.ToString()!;
                else
                    return null;
            }

            if (claimPath.First().Equals("const"))
            {
                 return claimPath[1];
            }

            return null;
        }

        public async Task<object?> GetKeyVal(string claimPathRaw)
        {
            string[] claimPath = claimPathRaw.Split(".");
            if(claimPath.First().Equals("NBSP"))
            {
                return " ";
            }

            if (claimPath.First().Equals("tag"))
            {
                try
                {
                    var domain = claimPath[1];
                    var entity = claimPath[2];
                    var tagName = claimPath[3];
                    var fieldName = claimPath[4];

                    var tagDataResponse = await _tagService.GetTagInfo(domain, entity, tagName, _queryStringForTag!);
                    if (tagDataResponse.StatusCode != 200)
                        return null;
                    var tagData = tagDataResponse.Response;

                    return tagData![fieldName]?.ToString() ?? null;

                }
                catch (Exception ex)
                {
                    Logger.LogError("Get Tag Info :" + ex.ToString());
                }
            }

            if (claimPath.First().Equals("user"))
            {
                if (_user == null)
                    return null;

                Type t = _user!.GetType();

                var property = t.GetProperties().FirstOrDefault(p => p.Name.ToLower() == claimPath[1]);
                if (property == null)
                    return null;
                if (property!.GetValue(_user!) == null)
                    return null;

                return property!.GetValue(_user!);
            }

            if (claimPath.First().Equals("openbanking"))
            {
                if (_consent == null)
                    return null;

                Type t = _consent!.GetType();

                var property = t.GetProperties().FirstOrDefault(p => p.Name.ToLower() == claimPath[2]);
                if (property == null)
                    return null;
                if (property.GetValue(_consent) == null)
                    return null;

                return property!.GetValue(_consent)!;
            }

            if (claimPath.First().Equals("profile"))
            {
                if (_profile == null)
                    return null;

                Type t = _profile!.GetType();

                var propValue = GetPropertyValue(_profile, string.Join('.', claimPath.ToList().Skip(1)));
                
                if (propValue != null)
                    return propValue!;
                else
                    return null;

            }

            if (claimPath.First().Equals("collection"))
            {
                if (_collectionUser == null)
                    return null;

                Type t = _collectionUser!.GetType();

                var propValue = GetPropertyValue(_collectionUser, string.Join('.', claimPath.ToList().Skip(1)));
                
                if (propValue != null)
                    return propValue!;
                else
                    return null;
            }

            if (claimPath.First().Equals("const"))
            {
                 return claimPath[1];
            }

            return null;
        }

        private object? GetPropertyValue(object? src, string propName)
        {
            if (src == null) throw new ArgumentException("Value cannot be null.", "src");
            if (propName == null) throw new ArgumentException("Value cannot be null.", "propName");

            if (propName.Contains("."))
            {
                var temp = propName.Split(new char[] { '.' }, 2);
                var property = src.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower() == temp[0].ToLower());
                if (property == null)
                    return null;
                src = property.GetValue(src);
                return GetPropertyValue(src, temp[1]);
            }
            else
            {
                var property = src.GetType().GetProperties().FirstOrDefault(p => p.Name.ToLower(new CultureInfo("en-US",false)) == propName.ToLower());
                if(property is {})
                {
                    if(property.PropertyType.IsEnum)
                    {
                        return property != null ? Convert.ToInt32(property.GetValue(src, null)).ToString() : null;
                    }
                    if(property.PropertyType == typeof(bool))
                    {
                        return property != null ? property.GetValue(src, null) : null;
                    }
                }
                
                return property != null ? property.GetValue(src, null)?.ToString() : null;
            }
        }
        public async Task<List<Claim>> PopulateClaims(List<string> clientClaims, LoginResponse? user, SimpleProfileResponse? profile = null, ConsentResponse? consent = null, core.Models.Collection.User? collectionUser = null)
        {
            _profile = profile;
            if (consent != null)
            {
                _consent = consent;
            }
            if(_collectionUser is not {})
            {
                _collectionUser = collectionUser;
            }
            if (user == null)
            {

            }
            else
            {
                _user = user;
                _queryStringForTag = string.Empty;
                _queryStringForTag += "?user_reference=" + _user!.Reference;
                _queryStringForTag += "&mail=" + _user!.EMail;
                _queryStringForTag += "&phone=" + _user!.MobilePhone!.ToString();
            }

            List<Claim> claims = new List<Claim>();

            foreach (var identityClaim in clientClaims)
            {
                var claimName = string.Empty;
                var claimPath = string.Empty;

                if(identityClaim.Contains("|"))
                {
                    claimName = identityClaim.Split("|")[0];
                    claimPath = identityClaim.Split("|")[1];
                }
                else
                {
                    claimName = identityClaim;
                    claimPath = identityClaim;
                }
                
                var claimValue = string.Empty;

                var claimValueArr = claimPath.Split("&");

                foreach(var claimValuePath in claimValueArr)
                {
                    var claimDetail = claimValuePath.Split("???");
                    var alternativeClaim = String.Empty;

                    string? primaryClaim;
                    if (claimDetail.Length == 1)
                    {
                        primaryClaim = claimDetail.First();
                    }
                    else
                    {
                        primaryClaim = claimDetail.First();
                        alternativeClaim = claimDetail[1];
                    }

                    var claimDetailValue = await GetClaimDetail(primaryClaim);

                    if (claimDetailValue != null)
                    {
                        claimValue += claimDetailValue;
                    }
                    else
                    {
                        claimDetailValue = await GetClaimDetail(alternativeClaim);
                        claimValue += claimDetailValue ?? string.Empty;
                    }
                }

                if(!string.IsNullOrWhiteSpace(claimValue))
                    claims.Add(new Claim(claimName, claimValue.Trim()));
                

            }

            return claims;
        }

        public async Task<List<KeyValuePair<string, object>?>> PopulatePrivateClaims(List<string> clientClaims, LoginResponse? user, SimpleProfileResponse? profile = null, ConsentResponse? consent = null, core.Models.Collection.User? collectionUser = null)
        {
            _profile = profile;
            if (consent != null)
            {
                _consent = consent;
            }
            if(_collectionUser is not {})
            {
                _collectionUser = collectionUser;
            }
            if (user == null)
            {

            }
            else
            {
                _user = user;
                _queryStringForTag = string.Empty;
                _queryStringForTag += "?reference=" + _user!.Reference;
                _queryStringForTag += "&mail=" + _user!.EMail;
                _queryStringForTag += "&phone=" + _user!.MobilePhone!.ToString();
            }

            List<KeyValuePair<string,object>?> claims = new List<KeyValuePair<string,object>?>();

            foreach (var identityClaim in clientClaims)
            {
                var claimName = string.Empty;
                var claimPath = string.Empty;

                if(identityClaim.Contains("|"))
                {
                    claimName = identityClaim.Split("|")[0];
                    claimPath = identityClaim.Split("|")[1];
                }
                else
                {
                    claimName = identityClaim;
                    claimPath = identityClaim;
                }
                
                object? claimValue;
                var claimValueString = string.Empty;

                var claimValueArr = claimPath.Split("&");

                if(claimValueArr.Count() > 1)
                {
                    foreach(var claimValuePath in claimValueArr)
                    {
                        var claimDetail = claimValuePath.Split("???");
                        var alternativeClaim = String.Empty;

                        string? primaryClaim;
                        if (claimDetail.Length == 1)
                        {
                            primaryClaim = claimDetail.First();
                        }
                        else
                        {
                            primaryClaim = claimDetail.First();
                            alternativeClaim = claimDetail[1];
                        }

                        var claimDetailValue = await GetKeyVal(primaryClaim);

                        if (claimDetailValue != null)
                        {
                            claimValueString += claimDetailValue.ToString();
                        }
                        else
                        {
                            claimDetailValue = await GetKeyVal(alternativeClaim);
                            claimValueString += claimDetailValue?.ToString() ?? string.Empty;
                        }
                    }
                    if(!string.IsNullOrWhiteSpace(claimValueString))
                        claims.Add(new KeyValuePair<string, object>(claimName, claimValueString.Trim()));
                }
                else
                {
                    var claimDetail = claimValueArr[0].Split("???");
                    var alternativeClaim = String.Empty;

                    string? primaryClaim;
                    if (claimDetail.Length == 1)
                    {
                        primaryClaim = claimDetail.First();
                    }
                    else
                    {
                        primaryClaim = claimDetail.First();
                        alternativeClaim = claimDetail[1];
                    }

                    var claimDetailValue = await GetKeyVal(primaryClaim);

                    if (claimDetailValue != null)
                    {
                        claimValue = claimDetailValue;
                    }
                    else
                    {
                        claimDetailValue = await GetKeyVal(alternativeClaim);
                        claimValue = claimDetailValue ?? null;
                    }
                    if(claimValue is not null)
                        claims.Add(new KeyValuePair<string, object>(claimName, claimValue));
                }

               

            }

            return claims;
        }

      
    }
}