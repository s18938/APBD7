using System.Text;
using AuthenticationSampleWebApp.Models;
using AuthenticationSampleWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationSampleWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<StudentDbService, SqlServerDbService>();
            services.AddTransient<StudentDbService, SqlServerDbService>();
            //HTTP Basic
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer=true,
                            ValidateAudience=true,
                            ValidateLifetime=true,
                            ValidIssuer="Gakko",
                            ValidAudience="Students",
                            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                        };
                    });

            //services.AddAuthentication("AuthenticationBasic")
            //      .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("AuthenticationBasic", null);

            services.AddDbContext<StudentDbContext>(options =>
            {
                options.UseSqlServer(@"Data Source=DESKTOP-5G2FL6J\SQLEXPRESS;Initial Catalog=APBD_4;Integrated Security=True");
            }
            );
            services.AddControllers()
                    .AddXmlSerializerFormatters();
            
            
            //content negotiation
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
