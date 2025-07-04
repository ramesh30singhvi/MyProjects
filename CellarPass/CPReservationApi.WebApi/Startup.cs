using CPReservationApi.WebApi.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CPReservationApi.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //private readonly IHostingEnvironment _appEnv;
        //public Startup(IHostingEnvironment env)
        //{
        //    var builder = new ConfigurationBuilder()
        //        .SetBasePath(env.ContentRootPath)
        //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        //        .AddEnvironmentVariables();
        //    Configuration = builder.Build();
        //    Common.Common.ConnectionString = Configuration.GetSection("ConnectionString").Value;
        //    Common.Common.ConnectionString_tablepro = Configuration.GetSection("ConnectionString_tablepro").Value;
        //    Common.Common.ConnectionString_readonly = Configuration.GetSection("ConnectionString_readonly").Value;
        //    _appEnv = env;
        //}

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            //Configure Options using Extension method 
            services.Configure<AppSettings>(option => Configuration.GetSection("AppSettings").Bind(option));

            Common.Common.ConnectionString = Configuration.GetSection("ConnectionString").Value;
            Common.Common.ConnectionString_tablepro = Configuration.GetSection("ConnectionString_tablepro").Value;
            Common.Common.ConnectionString_readonly = Configuration.GetSection("ConnectionString_readonly").Value;

            // Add framework services.
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddLogging();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen();
            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHsts();
            app.UseCors();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCookiePolicy();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());

            //loggerFactory.AddDebug();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CPReservation API V1");
            });
        }
    }
}
