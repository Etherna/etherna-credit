//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.ACR.Exceptions;
using Etherna.ACR.Settings;
using Etherna.CreditSystem.Configs;
using Etherna.CreditSystem.Configs.Authorization;
using Etherna.CreditSystem.Configs.Hangfire;
using Etherna.CreditSystem.Configs.Swagger;
using Etherna.CreditSystem.Domain;
using Etherna.CreditSystem.Extensions;
using Etherna.CreditSystem.Persistence;
using Etherna.CreditSystem.Services;
using Etherna.CreditSystem.Services.Settings;
using Etherna.CreditSystem.Services.Tasks;
using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.ServicesClient.AspNetCore;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Etherna.CreditSystem
{
    public class Startup
    {
        // Constructor.
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // Properties.
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // Methods.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
                })
                .SetApplicationName(CommonConsts.SharedCookieApplicationName);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = Configuration.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = knownNetworksConfig.Get<string[]>().Select(address =>
                    {
                        var parts = address.Split('/');
                        if (parts.Length != 2)
                            throw new ServiceConfigurationException();

                        return new IPNetwork(
                            IPAddress.Parse(parts[0]),
                            int.Parse(parts[1], CultureInfo.InvariantCulture));
                    });

                    foreach (var network in networks)
                        options.KnownNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorClaimPolicy);

                options.Conventions.AuthorizeAreaFolder("Deposit", "/");
                options.Conventions.AuthorizeAreaFolder("Manage", "/");
            });
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            // Configure authentication.
            var allowUnsafeAuthorityConnection = false;
            if (Configuration["SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(Configuration["SsoServer:AllowUnsafeConnection"]);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    // Set properties.
                    options.Cookie.MaxAge = TimeSpan.FromDays(30);
                    options.Cookie.Name = CommonConsts.SharedCookieApplicationName;
                    options.AccessDeniedPath = "/AccessDenied";

                    if (Environment.IsProduction())
                        options.Cookie.Domain = ".etherna.io";

                    // Handle unauthorized call on api with 401 response. For already logged in users.
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        else
                            context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => //client config
                {
                    // Set properties.
                    options.Authority = Configuration["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = Configuration["SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = Configuration["SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();
                    options.ResponseType = "code";

                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("role");

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;

                    // Handle unauthorized call on api with 401 response. For users not logged in.
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Audience = "ethernaCreditServiceInteract";
                    options.Authority = Configuration["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                });

            // Configure authorization.
            //policy and requirements
            services.AddAuthorization(options =>
            {
                //default policy
                options.DefaultPolicy = new AuthorizationPolicy(
                    new IAuthorizationRequirement[]
                    {
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    },
                    Array.Empty<string>());

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorClaimPolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ClaimTypes.Role, CommonConsts.AdministratorRoleName);
                    });

                options.AddPolicy(CommonConsts.ServiceInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { JwtBearerDefaults.AuthenticationScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "ethernaCredit_serviceInteract_api");
                });
            });

            //requirement handlers
            services.AddScoped<IAuthorizationHandler, DenyBannedAuthorizationHandler>();

            // Configure Hangfire server.
            if (!Environment.IsStaging()) //don't start server in staging
            {
                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues = new[]
                    {
                        Queues.DB_MAINTENANCE,
                        "default"
                    };
                    options.WorkerCount = System.Environment.ProcessorCount * 2;
                });
            }

            // Configure Swagger services.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();

                //add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                //integrate xml comments
                var xmlFile = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Configure Etherna SSO Client services.
            var ethernaServiceClientBuilder = services.AddEthernaSsoClientForServices(
                new Uri(Configuration["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()),
                Configuration["SsoServer:Clients:SsoServer:ClientId"] ?? throw new ServiceConfigurationException(),
                Configuration["SsoServer:Clients:SsoServer:Secret"] ?? throw new ServiceConfigurationException());

            var clientCredentialTask = ethernaServiceClientBuilder.GetClientCredentialsTokenRequestAsync(!allowUnsafeAuthorityConnection);
            clientCredentialTask.Wait();
            var clientCredential = clientCredentialTask.Result;

            // Register token manager.
            services.AddAccessTokenManagement(options =>
            {
                options.Client.Clients.Add(ethernaServiceClientBuilder.ClientName, clientCredential);
            });

            // Configure setting.
            var assemblyVersion = new AssemblyVersion(GetType().GetTypeInfo().Assembly);
            services.Configure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = assemblyVersion.Version;
            });
            services.Configure<EmailSettings>(Configuration.GetSection("Email") ?? throw new ServiceConfigurationException());
            services.Configure<SsoServerSettings>(Configuration.GetSection("SsoServer") ?? throw new ServiceConfigurationException());

            // Configure persistence.
            services.AddMongODMWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
                options.StorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                };
            }, configureMongODMOptions: options =>
            {
                options.DbMaintenanceQueueName = Queues.DB_MAINTENANCE;
            })
                .AddDbContext<ICreditDbContext, CreditDbContext>(sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    return new CreditDbContext(eventDispatcher);
                },
                options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:CreditDb"] ?? throw new ServiceConfigurationException();
                    options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
                })

                .AddDbContext<ISharedDbContext, SharedDbContext>(options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                    options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
                });

            services.AddMongODMAdminDashboard(new MongODM.AspNetCore.UI.DashboardOptions
            {
                AuthFilters = new[] { new Configs.MongODM.AdminAuthFilter() },
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure domain services.
            services.AddDomainServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiProvider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                if (Environment.IsDevelopment())
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
                else
                {
                    builder.WithOrigins("https://etherna.io")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add Hangfire.
            app.UseHangfireDashboard(
                CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    Authorization = new[] { new AdminAuthFilter() }
                });

            // Add Swagger and SwaggerUI.
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                //build a swagger endpoint for each discovered API version
                foreach (var description in apiProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            // Add pages and controllers.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
