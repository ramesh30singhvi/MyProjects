using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SHARP.DAL.Models;
using SHARP.DAL;
using SHARP.BusinessLogic.Services;
using SHARP.BusinessLogic.Interfaces;
using ApplicationUser = SHARP.DAL.Models.ApplicationUser;
using System;
using SHARP.Middlewares;
using SHARP.Extensions;
using FluentValidation.AspNetCore;
using System.Reflection;
using SHARP.BusinessLogic;
using Microsoft.ApplicationInsights.Extensibility;
using SHARP.Common.Models;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;


namespace SHARP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.RegisterAutoMapper();

            SHARPContext.ConnectionString = Configuration.GetConnectionString("ConnStr");

            var maxFailedAccessAttempts = string.IsNullOrEmpty(Configuration["MaxFailedAccessAttempts"]) ? 3 : Int32.Parse(Configuration["MaxFailedAccessAttempts"]);
            var minutes = string.IsNullOrEmpty(Configuration["DefaultLockoutTimeSpan"]) ? 5 : Int32.Parse(Configuration["DefaultLockoutTimeSpan"]);
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Lockout.MaxFailedAccessAttempts = maxFailedAccessAttempts;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(minutes);
                })
                .AddEntityFrameworkStores<SHARPContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<TwoFATokenProvider>(TwoFATokenProvider.NAME);

            services.AddCors();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ASP.NET 5 Web API",
                    Description = "Authentication and Authorization in ASP.NET 5 with JWT and Swagger"
                });
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHttpContextAccessor();
            //services.AddHttpClient<ReportService>().ConfigurePrimaryHttpMessageHandler(() =>
            //{
            //    return new SocketsHttpHandler()
            //    {
            //        PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            //        ConnectTimeout = TimeSpan.FromMinutes(15)
                   

            //    };
            //})
            //.SetHandlerLifetime(Timeout.InfiniteTimeSpan);

            services.AddDbContext<SHARPContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<AppConfig>();

            services.AddTransient<IUserInfoService, UserInfoService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IFEClientService, FEClientService>();
            services.AddTransient<IEmailClient, EmailClient>();
            services.AddTransient<IAuditService, AuditService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IOrganizationService, OrganizationService>();
            services.AddTransient<IFacilityService, FacilityService>();
            services.AddTransient<IFormService, FormService>();
            services.AddTransient<ITableauServerService, TableauServerService>();
            services.AddTransient<ITableauReportService, TableauReportService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<IMemoService, MemoService>();
            services.AddTransient<IDasbhoardInputService, DashboardInputService>();
            services.AddTransient<IReportRequestService, ReportRequestService>();
            services.AddTransient<IReportAIService, ReportAIService>();
            services.AddTransient<IOpenAIService, OpenAIService>();
            services.AddTransient<IHighAlertService, HighAlertService>();
            services.AddTransient<IPortalReportService, PortalReportService>();
            services.AddTransient<IAuditorProductivityDashboardService, AuditorProductivityDashboardService>();


            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist/monster-admin-angular";
            });

            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromMinutes(5));

            services.AddMvc(i => i.EnableEndpointRouting = false)
            .AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            });

            services.AddApplicationInsightsTelemetry();
            services.AddAppInsightsHttpBodyLogging(o => o.Configure(Configuration));

            services.Configure<TelemetryConfiguration>(
            (o) => {
                string connectionString = Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");
                if(!string.IsNullOrEmpty(connectionString))
                {
                    o.ConnectionString = connectionString;
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SHARP v1"));
            }
            else
            {
                app.UseMiddleware<BodyLoggerMiddleware>();
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //var webSocketOptions = new WebSocketOptions
            //{
            //    KeepAliveInterval = TimeSpan.FromMinutes(20)
            //};

           // app.UseWebSockets(webSocketOptions);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                
                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = new TimeSpan(days: 0, hours: 0, minutes: 3, seconds: 30);
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
