

using amorphie.token.core.Dtos;

namespace amorphie.token.Services.User;

public class UserService : ServiceBase, IUserService
{
    private readonly DaprClient _daprClient;
    public UserService(DaprClient daprClient, ILogger<UserService> logger, IConfiguration configuration) : base(logger, configuration)
    {
        _daprClient = daprClient;
    }

    public async Task<ServiceResponse<object>> CheckDevice(Guid userId, string clientId, string deviceId, Guid installationId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Get, Configuration["UserServiceAppName"], $"/userDevice/check-device/{clientId}/{userId}/{deviceId}/{installationId}");
            return new ServiceResponse<object>()
            {
                StatusCode = 200,
                Detail = "",
                Response = null
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 404)
                {
                    return new ServiceResponse<object>()
                    {
                        StatusCode = 404,
                        Detail = "User Device Not Found"
                    };
                }
                return new ServiceResponse<object>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = "User Device Not Found"
                };
            }
            else
            {
                Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<object>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<object>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }

    public async Task<ServiceResponse<CheckDeviceWithoutUserResponseDto>> CheckDeviceWithoutUser(string clientId, string deviceId, Guid installationId)
    {
        try
        {
            var res = await _daprClient.InvokeMethodAsync<CheckDeviceWithoutUserResponseDto>(HttpMethod.Get, Configuration["UserServiceAppName"], $"/userDevice/check-device-without-user/{clientId}/{deviceId}/{installationId}");
            return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
            {
                StatusCode = 200,
                Detail = "",
                Response = res
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 404)
                {
                    return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
                    {
                        StatusCode = 404,
                        Detail = "User Device Not Found"
                    };
                }
                return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = "User Device Not Found"
                };
            }
            else
            {
                Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<CheckDeviceWithoutUserResponseDto>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }

    public async Task<ServiceResponse<LoginResponse>> Login(LoginRequest loginRequest)
    {
        try
        {
            var user = await _daprClient.InvokeMethodAsync<LoginRequest, LoginResponse>(Configuration["UserServiceAppName"], "/user/login",
            new()
            {
                Reference = loginRequest.Reference,
                Password = loginRequest.Password
            });
            if (user == null)
            {
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 460,
                    Detail = "User Not Found"
                };
            }
            return new ServiceResponse<LoginResponse>()
            {
                StatusCode = 200,
                Detail = "",
                Response = user
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 460)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 460,
                        Detail = "User Not Found"
                    };
                }
                if ((int)ex.Response.StatusCode == 461)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 461,
                        Detail = "Invalid Reference or Password"
                    };
                }
            }
            else
            {
                Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<LoginResponse>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }

    public async Task<ServiceResponse<LoginResponse>> GetUserById(Guid userId)
    {
        try
        {
            var user = await _daprClient.InvokeMethodAsync<LoginResponse>(HttpMethod.Get, Configuration["UserServiceAppName"], "/user/" + userId);
            if (user == null)
            {
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 460,
                    Detail = "User Not Found"
                };
            }
            return new ServiceResponse<LoginResponse>()
            {
                StatusCode = 200,
                Detail = "",
                Response = user
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 460)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 460,
                        Detail = "User Not Found"
                    };
                }
                if ((int)ex.Response.StatusCode == 461)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 461,
                        Detail = "Invalid Reference or Password"
                    };
                }
            }
            else
            {
                Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<LoginResponse>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }


    public async Task<ServiceResponse> SaveUser(UserInfo userInfo)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/user", userInfo);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "Success"
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse> SaveDevice(UserSaveMobileDeviceDto userSaveMobileDeviceDto)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/userDevice/save-mobile-device-client", userSaveMobileDeviceDto);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Save Device | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Save Device | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Save Device Invoke Method Error"
            };
        }

    }

    public async Task<ServiceResponse> RemoveDevice(string reference, string clientId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Put, Configuration["UserServiceAppName"], "/public/device/remove/" + clientId + "/" + reference);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Remove Device | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Remove Device | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Remove Device Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse<LoginResponse>> GetUserByReference(string reference)
    {
        try
        {
            var user = await _daprClient.InvokeMethodAsync<LoginResponse>(HttpMethod.Get, Configuration["UserServiceAppName"], "/user/reference/" + reference);
            if (user == null)
            {
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 460,
                    Detail = "User Not Found"
                };
            }
            return new ServiceResponse<LoginResponse>()
            {
                StatusCode = 200,
                Detail = "",
                Response = user
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 460)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 460,
                        Detail = "User Not Found"
                    };
                }
                if ((int)ex.Response.StatusCode == 461)
                {
                    return new ServiceResponse<LoginResponse>()
                    {
                        StatusCode = 461,
                        Detail = "Invalid Reference or Password"
                    };
                }
            }
            else
            {
                Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<LoginResponse>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<LoginResponse>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };

    }

    public async Task<ServiceResponse> MigrateSecurityQuestion(MigrateSecurityQuestionRequest migrateSecurityQuestionRequest)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/userSecurityQuestion/migrate", migrateSecurityQuestionRequest);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Question | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Question | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Migrate Security Question Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse> MigrateSecurityImage(MigrateSecurityImageRequest migrateSecurityImageRequest)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/userSecurityImage/migrate", migrateSecurityImageRequest);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Image | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Image | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Migrate Security Image Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse> MigrateSecurityImages(List<SecurityImageRequestDto> securityImageRequestDtos)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/userSecurityImage/migrateImages", securityImageRequestDtos);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Images | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Images | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Migrate Security Image Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse> MigrateSecurityQuestions(List<SecurityQuestionRequestDto> securityQuestionRequestDtos)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(Configuration["UserServiceAppName"], "/userSecurityQuestion/migrateQuestions", securityQuestionRequestDtos);
            return new ServiceResponse()
            {
                StatusCode = 200,
                Detail = "",
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Questions | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured At User Invocation Migrate Security Questions | Detail:" + ex.ToString());
            return new ServiceResponse()
            {
                StatusCode = 500,
                Detail = "Dapr Migrate Security Image Invoke Method Error"
            };
        }
    }

    public async Task<ServiceResponse<GetPublicDeviceDto>> GetPublicDevice(string clientCode, string reference)
    {
        try
        {
            var res = await _daprClient.InvokeMethodAsync<GetPublicDeviceDto>(HttpMethod.Get, Configuration["UserServiceAppName"], $"/public/device/{clientCode}/{reference}");
            return new ServiceResponse<GetPublicDeviceDto>()
            {
                StatusCode = 200,
                Detail = "Success",
                Response = res
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 404)
                {
                    return new ServiceResponse<GetPublicDeviceDto>()
                    {
                        StatusCode = 404,
                        Detail = "User Device Not Found"
                    };
                }
                return new ServiceResponse<GetPublicDeviceDto>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = "User Device Not Found"
                };
            }
            else
            {
                Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<GetPublicDeviceDto>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<GetPublicDeviceDto>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }

    public async Task<ServiceResponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions()
    {
        try
        {
            var questions = await _daprClient.InvokeMethodAsync<IEnumerable<SecurityQuestionDto>>(HttpMethod.Get,Configuration["UserServiceAppName"], "/securityQuestion/getAll");
            return new ServiceResponse<IEnumerable<SecurityQuestionDto>>()
            {
                StatusCode = 200,
                Detail = "",
                Response = questions
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Security Questions | Invocation Failed | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<SecurityQuestionDto>>()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Security Questions | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<SecurityQuestionDto>>()
            {
                StatusCode = 500,
                Detail = "An Error Occured When Trying To Retrieve Security Questions | Detail:" + ex.ToString()
            };
        }
    }

    public async Task<ServiceResponse<IEnumerable<SecurityImageDto>>> GetSecurityImages()
    {
        try
        {
            var questions = await _daprClient.InvokeMethodAsync<IEnumerable<SecurityImageDto>>(HttpMethod.Get,Configuration["UserServiceAppName"], "/securityImage/getAll");
            return new ServiceResponse<IEnumerable<SecurityImageDto>>()
            {
                StatusCode = 200,
                Detail = "",
                Response = questions
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Security Images | Invocation Failed | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<SecurityImageDto>>()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Security Images | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<SecurityImageDto>>()
            {
                StatusCode = 500,
                Detail = "An Error Occured When Trying To Retrieve Security Images | Detail:" + ex.ToString()
            };
        }
    }

    public async Task<ServiceResponse<UserSecurityQuestionDto>> GetLastSecurityQuestion(Guid id)
    {
        try
        {
            var questions = await _daprClient.InvokeMethodAsync<UserSecurityQuestionDto>(HttpMethod.Get,Configuration["UserServiceAppName"], "/userSecurityQuestion/getLastSecurityQuestion/"+id);
            return new ServiceResponse<UserSecurityQuestionDto>()
            {
                StatusCode = 200,
                Detail = "",
                Response = questions
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Last Security Question | Invocation Failed | Detail:" + ex.ToString());
            return new ServiceResponse<UserSecurityQuestionDto>()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Last Security Question | Detail:" + ex.ToString());
            return new ServiceResponse<UserSecurityQuestionDto>()
            {
                StatusCode = 500,
                Detail = "An Error Occured When Trying To Retrieve Last Security Question | Detail:" + ex.ToString()
            };
        }
    }

    public async Task<ServiceResponse<UserSecurityImageDto>> GetLastSecurityImage(Guid id)
    {
        try
        {
            var questions = await _daprClient.InvokeMethodAsync<UserSecurityImageDto>(HttpMethod.Get,Configuration["UserServiceAppName"], "/userSecurityImage/getLastSecurityImage/"+id);
            return new ServiceResponse<UserSecurityImageDto>()
            {
                StatusCode = 200,
                Detail = "",
                Response = questions
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Last Security Image | Invocation Failed | Detail:" + ex.ToString());
            return new ServiceResponse<UserSecurityImageDto>()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve Last Security Image | Detail:" + ex.ToString());
            return new ServiceResponse<UserSecurityImageDto>()
            {
                StatusCode = 500,
                Detail = "An Error Occured When Trying To Retrieve Last Security Image | Detail:" + ex.ToString()
            };
        }
    }

    public async Task<ServiceResponse<IEnumerable<UserClaimDto>>> GetUserClaims(Guid userId)
    {
        try
        {
            var userClaims = await _daprClient.InvokeMethodAsync<IEnumerable<UserClaimDto>>(HttpMethod.Get,Configuration["UserServiceAppName"], "/userClaim/getByUserId/"+userId);
            return new ServiceResponse<IEnumerable<UserClaimDto>>()
            {
                StatusCode = 200,
                Detail = "",
                Response = userClaims
            };
        }
        catch (InvocationException ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve User Claims | Invocation Failed | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<UserClaimDto>>()
            {
                StatusCode = (int)ex.Response.StatusCode,
                Detail = await ex.Response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("An Error Occured When Trying To Retrieve User Claims | Detail:" + ex.ToString());
            return new ServiceResponse<IEnumerable<UserClaimDto>>()
            {
                StatusCode = 500,
                Detail = "An Error Occured When Trying To Retrieve User Claims | Detail:" + ex.ToString()
            };
        }
    }

    public async Task<ServiceResponse<ActiveDeviceDto>> CheckDevice(string reference, string clientId)
    {
        try
        {
            var response = await _daprClient.InvokeMethodAsync<ActiveDeviceDto>(HttpMethod.Get, Configuration["UserServiceAppName"], $"/public/device/{clientId}/{reference}");
            return new ServiceResponse<ActiveDeviceDto>()
            {
                StatusCode = 200,
                Detail = "",
                Response = response
            };
        }
        catch (InvocationException ex)
        {

            if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode < 500)
            {
                if ((int)ex.Response.StatusCode == 404)
                {
                    return new ServiceResponse<ActiveDeviceDto>()
                    {
                        StatusCode = 404,
                        Detail = "User Device Not Found"
                    };
                }
                return new ServiceResponse<ActiveDeviceDto>()
                {
                    StatusCode = (int)ex.Response.StatusCode,
                    Detail = "User Device Not Found"
                };
            }
            else
            {
                Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
                return new ServiceResponse<ActiveDeviceDto>()
                {
                    StatusCode = 500,
                    Detail = "Server Error"
                };
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("An Error Occured At User Device Invocation | Detail:" + ex.ToString());
        }
        return new ServiceResponse<ActiveDeviceDto>()
        {
            StatusCode = 500,
            Detail = "Server Error"
        };
    }
}
