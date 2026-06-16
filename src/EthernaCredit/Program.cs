// Copyright 2021-present Etherna SA
// This file is part of Etherna Credit.
// 
// Etherna Credit is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Credit is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Credit.
// If not, see <https://www.gnu.org/licenses/>.

using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Etherna.ACR.Exceptions;
using Etherna.ACR.Middlewares.DebugPages;
using Etherna.ACR.Settings;
using Etherna.Authentication.AspNetCore;
using Etherna.Credit.Areas.Api;
using Etherna.Credit.Configs;
using Etherna.Credit.Configs.Authorization;
using Etherna.Credit.Configs.ModelBinding;
using Etherna.Credit.Configs.MongODM;
using Etherna.Credit.Configs.OpenApi;
using Etherna.Credit.Domain;
using Etherna.Credit.Extensions;
using Etherna.Credit.Persistence;
using Etherna.Credit.Services;
using Etherna.Credit.Services.Settings;
using Etherna.Credit.Services.Tasks;
using Etherna.Credit.Services.Tasks.Infrastructure.Cron;
using Etherna.DomainEvents;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.Sdk.Internal.AspNetCore;
using Etherna.SwarmSdk.JsonConverters;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DashboardOptions = Etherna.MongODM.AspNetCore.UI.DashboardOptions;
using IPNetwork = System.Net.IPNetwork;

namespace Etherna.Credit
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Configure logging first.
            ConfigureLogging();

            // Then create the host, so that if the host fails we can log errors.
            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Configs.
                builder.Host.UseSerilog();

                ConfigureServices(builder);

                var app = builder.Build();
                ConfigureApplication(app);

                // First operations.
                app.SeedDbContexts();

                // Run application.
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Helpers.
        private static void ConfigureLogging()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new ServiceConfigurationException();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var elasticNodes = (configuration.GetSection("Elastic:Urls").Get<string[]>() ?? throw new ServiceConfigurationException())
                .Select(u => new Uri(u))
                .ToArray();
            var elasticUsername = configuration["Elastic:Username"];
            var elasticPassword = configuration["Elastic:Password"];
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name!.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);
            var envName = env.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Elasticsearch(elasticNodes, opts =>
                {
                    opts.BootstrapMethod = BootstrapMethod.Silent;
                    opts.DataStream = new DataStreamName("logs", assemblyName, envName);
                }, transport =>
                {
                    // Apply basic auth only when credentials are configured, so the same build
                    // runs against both the unsecured cluster (no creds) and the secured one
                    // (Elastic:Username/Password set via env).
                    if (!string.IsNullOrEmpty(elasticUsername) && !string.IsNullOrEmpty(elasticPassword))
                        transport.Authentication(new BasicAuthentication(elasticUsername, elasticPassword));
                })
                .Enrich.WithProperty("Environment", env)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var config = builder.Configuration;
            var env = builder.Environment;

            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
                })
                .SetApplicationName(CommonConsts.SharedCookieApplicationName);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = config.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = knownNetworksConfig.Get<string[]>()?.Select(address =>
                    {
                        var parts = address.Split('/');
                        if (parts.Length != 2)
                            throw new ServiceConfigurationException();

                        return new IPNetwork(
                            IPAddress.Parse(parts[0]),
                            int.Parse(parts[1], CultureInfo.InvariantCulture));
                    }) ?? [];

                    foreach (var network in networks)
                        options.KnownIPNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddOpenApi("Credit03", options =>
            {
                options.AddDocumentTransformer(new CreditDocumentTransformer(
                    config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()));
                options.AddDocumentTransformer<MetadataFilterDocumentTransformer<CreditApiMarker>>();

                options.AddOperationTransformer<ApiMethodNeedsAuthOperationTransformer>();
                options.AddOperationTransformer<DeprecatedOperationTransformer>();
                options.AddOperationTransformer<RemoveDefaultResponse200OperationTransformer>();
                options.AddOperationTransformer<CreditOperationTransformer>();
                
                options.AddSchemaTransformer(new SwarmModelsSchemaTransformer(xdaiFormat: NumericFormat.AsFloat));
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorRolePolicy);

                options.Conventions.AuthorizeAreaFolder(CommonConsts.DepositArea, "/");
                options.Conventions.AuthorizeAreaFolder(CommonConsts.ManageArea, "/");
                options.Conventions.AuthorizeAreaFolder(CommonConsts.WithdrawArea, "/");
            })
            .AddMvcOptions(options =>
                options.ModelBinderProviders.Insert(0, new XDaiValueModelBinderProvider()));
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new EthAddressJsonConverter());
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new XDaiValueJsonConverter(NumericFormat.AsFloat));

                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // Configure authentication.
            var allowUnsafeAuthorityConnection = false;
            if (config["SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(config["SsoServer:AllowUnsafeConnection"]!);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CommonConsts.UserAuthenticationPolicyScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })

                //users access
                .AddCookie(CommonConsts.UserAuthenticationCookieScheme, options =>
                {
                    // Set properties.
                    options.Cookie.MaxAge = TimeSpan.FromDays(30);
                    options.Cookie.Name = CommonConsts.SharedCookieApplicationName;
                    options.AccessDeniedPath = "/AccessDenied";

                    if (env.IsProduction())
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
                .AddJwtBearer(CommonConsts.UserAuthenticationJwtScheme, options =>
                {
                    options.Audience = "userApi";
                    options.Authority = config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                })
                .AddPolicyScheme(CommonConsts.UserAuthenticationPolicyScheme, CommonConsts.UserAuthenticationPolicyScheme, options =>
                {
                    //runs on each request
                    options.ForwardDefaultSelector = context =>
                    {
                        //filter by auth type
                        string? authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return CommonConsts.UserAuthenticationJwtScheme;

                        //otherwise always check for cookie auth
                        return CommonConsts.UserAuthenticationCookieScheme;
                    };
                })
                .AddEthernaOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // Set properties.
                    options.Authority = config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = config["SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = config["SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("role");

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

                //services access
                .AddJwtBearer(CommonConsts.ServiceAuthenticationScheme, options =>
                {
                    options.Audience = "ethernaCreditServiceInteract";
                    options.Authority = config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                });

            // Configure authorization.
            //policy and requirements
            services.AddAuthorization(options =>
            {
                //default policy
                options.DefaultPolicy = new AuthorizationPolicy(
                    [
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    ],
                    []);

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorRolePolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                        policy.AddRequirements(new RequireRoleAuthorizationRequirement(
                            CommonConsts.AdministratorRoleName));
                    });
                
                options.AddPolicy(CommonConsts.UserInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { CommonConsts.UserAuthenticationJwtScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "userApi.credit");
                    policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                });

                options.AddPolicy(CommonConsts.ServiceInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = new List<string> { CommonConsts.ServiceAuthenticationScheme };
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "ethernaCredit_serviceInteract_api");
                });
            });

            //requirement handlers
            services.AddScoped<IAuthorizationHandler, DenyBannedAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, RequireRoleAuthorizationHandler>();

            // Configure Hangfire server.
            if (!env.IsStaging()) //don't start server in staging
            {
                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues =
                    [
                        Queues.DB_MAINTENANCE,
                        "default"
                    ];
                    options.WorkerCount = Environment.ProcessorCount * 2;
                });
            }

            // Configure Etherna SSO Client services.
            services.AddEthernaInternalClients(
                new Uri(config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()),
                !allowUnsafeAuthorityConnection)
                .AddEthernaSsoClient(
                    config["SsoServer:Clients:SsoServer:ClientId"] ?? throw new ServiceConfigurationException(),
                    config["SsoServer:Clients:SsoServer:Secret"] ?? throw new ServiceConfigurationException());

            // Configure setting.
            services.Configure<EmailSettings>(config.GetSection("Email") ?? throw new ServiceConfigurationException());
            services.Configure<SsoServerSettings>(config.GetSection("SsoServer") ?? throw new ServiceConfigurationException());
            
            // Configure api handler.
            services.AddScoped<ICreditApiHandler, CreditApiHandler>();

            // Configure persistence.
            services.AddMongODMWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = config["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
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
                    var logger = sp.GetRequiredService<ILogger<CreditDbContext>>();
                    return new CreditDbContext(eventDispatcher, logger);
                },
                options =>
                {
                    options.ConnectionString = config["ConnectionStrings:CreditDb"] ?? throw new ServiceConfigurationException();
                })

                .AddDbContext<ISharedDbContext, SharedDbContext>(options =>
                {
                    options.ConnectionString = config["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                });

            services.AddMongODMAdminDashboard(new DashboardOptions
            {
                AuthFilters = [new AdminAuthFilter()],
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure domain services.
            services.AddDomainServices();
        }
        
        private static void ConfigureApplication(WebApplication app)
        {
            var config = app.Configuration;
            var env = app.Environment;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
                app.UseEthernaAcrDebugPages();
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
                if (env.IsDevelopment())
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

            // Add api and pages.
            app.MapOpenApi();
            app.MapRazorPages();

            app.MapCreditApi();

            // Add Hangfire.
            app.UseHangfireDashboard(
                CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    Authorization = [new Configs.Hangfire.AdminAuthFilter()]
                });

            // Add Scalar API Reference.
            app.MapScalarApiReference((options, httpContext) =>
            {
                options.WithTitle("Etherna Credit API")
                    .WithOpenApiRoutePattern("/openapi/credit03.json")
                    .DisableAgent()
                    .AddPreferredSecuritySchemes("OAuth")
                    .AddAuthorizationCodeFlow("OAuth", flow =>
                    {
                        flow.ClientId = config["SsoServer:Clients:Scalar:ClientId"] ?? throw new ServiceConfigurationException();
                        flow.RedirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/scalar/credit03";
                        flow.Pkce = Pkce.Sha256;
                        flow.SelectedScopes = ["openid", "profile", "ether_accounts", "role", "userApi.credit"];
                    });
            });
            
            // Register cron tasks.
            //infrastructure.
            RecurringJob.AddOrUpdate<ICleanupOldFailedTasksTask>(
                CleanupOldFailedTasksTask.TaskId,
                task => task.RunAsync(),
                Cron.Daily);
        }
    }
}