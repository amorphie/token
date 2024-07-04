using System.Text;
using System.Text.Json;
using amorphie.core.Extension;
using amorphie.token;
using amorphie.token.core;
using amorphie.token.data;
using amorphie.token.Middlewares;
using amorphie.token.Modules.Login;
using amorphie.token.Services.Card;
using amorphie.token.Services.ClaimHandler;
using amorphie.token.Services.Consent;
using amorphie.token.Services.FlowHandler;
using amorphie.token.Services.InternetBanking;
using amorphie.token.Services.LegacySSO;
using amorphie.token.Services.Login;
using amorphie.token.Services.MessagingGateway;
using amorphie.token.Services.Migration;
using amorphie.token.Services.Profile;
using amorphie.token.Services.Role;
using amorphie.token.Services.TransactionHandler;
using Elastic.Apm.NetCoreAll;
using Microsoft.EntityFrameworkCore;
using Refit;
using Serilog;
using Serilog.Formatting.Compact;

internal partial class Program
{
   
    private static async Task Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();
        var client = new DaprClientBuilder().Build();
        using (var tokenSource = new CancellationTokenSource(20000))
        {
            try
            {
                await client.WaitForSidecarAsync(tokenSource.Token);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Dapr Sidecar Doesn't Respond " + ex.ToString());
                return;
            }

        }


        await builder.Configuration.AddVaultSecrets(builder.Configuration["DAPR_SECRET_STORE_NAME"], new string[] { "ServiceConnections" });

        builder.Services.AddAntiforgery(x =>
        {
            x.SuppressXFrameOptionsHeader = true;
        });

        // Add services to the container.
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.WithOrigins("*")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
        builder.Logging.ClearProviders();
        Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.File(new CompactJsonFormatter(), "amorphie-token-log.json", rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
        builder.Host.UseSerilog(Log.Logger, true);

        builder.Services.AddHealthChecks();
        builder.Services.AddControllersWithViews();
        builder.Services.AddDaprClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(opt =>
        {
            opt.Cookie.Name = ".amorphie.token";
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<DatabaseContext>
            (options => options.UseNpgsql(builder.Configuration["DatabaseConnection"], b => b.MigrationsAssembly("amorphie.token.data")));
        builder.Services.AddDbContext<IbDatabaseContext>((sp,options) => 
        {
            try
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if(httpContext is not {})
                {
                    throw new Exception();
                }
                var request = httpContext!.Request;
                if(request.Path.ToString().Equals("/public/Login"))
                {
                    var formData =  request.ReadFormAsync().GetAwaiter().GetResult();
                    var code = formData!["code"];
                    if(string.IsNullOrWhiteSpace(code))
                    {
                        throw new Exception();
                    }

                    
                    var authorizationCodeInfo = client.GetStateAsync<AuthorizationCode>(builder.Configuration["DAPR_STATE_STORE_NAME"], code).GetAwaiter().GetResult();
                    if(authorizationCodeInfo.ClientId!.Equals("609023ad-e525-483b-a167-42402ef2195c"))
                    {
                        options.UseSqlServer(builder.Configuration["IbDatabaseCbTestConnection"]);
                    }
                    else
                    {
                        options.UseSqlServer(builder.Configuration["IbDatabaseConnection"]);
                    }
                    
                }
                else
                {
                    options.UseSqlServer(builder.Configuration["IbDatabaseConnection"]);
                }
                
                
            }
            catch (System.Exception)
            {
                options.UseSqlServer(builder.Configuration["IbDatabaseConnection"]);
            }
            
        });
        
        builder.Services.AddDbContext<IbSecurityDatabaseContext>(options => options.UseSqlServer(builder.Configuration["IbSecurityDatabaseConnection"]));
        builder.Services.AddDbContext<IbDatabaseContextMordor>(options => options.UseSqlServer(builder.Configuration["IbDatabaseMordorConnection"]));
        builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddScoped<IClientService, ClientServiceLocal>();
            builder.Services.AddScoped<IUserService, UserServiceLocal>();
            builder.Services.AddScoped<ITagService, TagServiceLocal>();
            builder.Services.AddScoped<IConsentService, ConsentServiceLocal>();
            builder.Services.AddScoped<IRoleService, RoleServiceLocal>();


            builder.Services.AddHttpClient("Client", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["ClientBaseAddress"]!);
            });
            builder.Services.AddHttpClient("User", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["UserBaseAddress"]!);
            });
            builder.Services.AddHttpClient("Tag", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["TagBaseAddress"]!);
            });
            builder.Services.AddHttpClient("Consent", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["ConsentBaseAddress"]!);
            });

            builder.Services.AddHttpClient("Role", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["RoleBaseAddress"]!);
            });

        }
        else
        {
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            //builder.Services.AddScoped<IConsentService, ConsentService>();
            builder.Services.AddScoped<IConsentService, ConsentServiceLocal>();
            builder.Services.AddHttpClient("Consent", httpClient =>
            {
                httpClient.BaseAddress = new Uri(builder.Configuration["ConsentBaseAddress"]!);
            });
        }

        builder.Services.AddHttpClient("SSO", httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["LegacySSOServiceBaseAddress"]!);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
            (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            }
        });

        builder.Services.AddScoped<IInternetBankingUserService, InternetBankingUserService>();
        builder.Services.AddScoped<IProfileService, ProfileService>();
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IFlowHandler, FlowHandler>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IClaimHandlerService, ClaimHandlerService>();
        builder.Services.AddScoped<ICardHandler, CardHandler>();

        builder.Services.AddTransient<IPasswordRememberService, PasswordRememberService>();
        builder.Services.AddTransient<IEkycProvider, EkycProvider>();
        builder.Services.AddTransient<IEkycService, EkycService>();
        builder.Services.AddTransient<ServiceCaller>();

        builder.Services.AddScoped<IMigrationService, MigrationService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        builder.Services.AddScoped<ILegacySSOService, LegacySSOService>();





        builder.Services.AddRefitClient<IProfile>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["ProfileBaseAddress"]!))
        .ConfigurePrimaryHttpMessageHandler(() => { return new HttpClientHandler() { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; } }; });

        builder.Services.AddRefitClient<ISimpleProfile>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["SimpleProfileBaseAddress"]!))
        .ConfigurePrimaryHttpMessageHandler(() => { return new HttpClientHandler() { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; } }; });

        builder.Services.AddRefitClient<ICardService>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["CardServiceBaseAddress"]!));

        builder.Services.AddRefitClient<IMessagingGateway>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["MessagingGatewayBaseAddress"]!));

        builder.Services.AddRefitClient<IPasswordRememberCard>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(builder.Configuration["cardValidationUri"]!));

        builder.Services.AddHttpClient("Enqura", httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["EnquraBaseAddress"]!);
        });


        builder.Services.AddHttpClient("MevduatStatusCheck", httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["EkycMevduatStatusCheckAddress"]!);
        });

        // Bind options from configuration :)
        builder.Services.AddOptions<CardValidationOptions>()
        .Bind(builder.Configuration.GetSection("CardValidation"));

        var app = builder.Build();
        app.UseAllElasticApm(app.Configuration);

        app.UseTransactionMiddleware();

        //Db Migrate
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        db.Database.Migrate();
      
        var migrateService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
        await migrateService.MigrateStaticData();

        app.MapHealthChecks("/health");

        app.MapLoginWorkflowEndpoints();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            //app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }


        app.UseCors();
        app.UseAntiforgery();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseSession();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.UseSwagger();
        app.UseSwaggerUI();

        app.Run();
    }
}