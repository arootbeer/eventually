using System.Threading;
using Eventually.Portal.UI.Areas.Identity;
using Eventually.Portal.UI.Areas.Identity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Eventually.Portal.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDatabaseDeveloperPageExceptionFilter()
                .AddRazorPages()
                .AddNewtonsoftJson(o => o.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            services.AddServerSideBlazor();
            services
                .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<PortalUser>>()
                .AddScoped<CancellationTokenSource>()
                .AddSingleton<IUserStore<PortalUser>, UserStore>()
                .AddSingleton<IRoleStore<PortalRole>, UserStore>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddDefaultIdentity<PortalUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<PortalRole>();
            services
                .AddAuthentication();
                // .AddGoogle(options =>
                // {
                //     var googleAuthNSection =
                //         Configuration.GetSection("Authentication:Google");
                //
                //     options.ClientId = googleAuthNSection["ClientId"];
                //     options.ClientSecret = googleAuthNSection["ClientSecret"];
                // });;

            services.Replace(new ServiceDescriptor(typeof(UserManager<PortalUser>), typeof(UserManager), ServiceLifetime.Scoped));
            services
                .AddScoped<Areas.Identity.SignInManager<PortalUser>, SignInManager>()
                .AddScoped<SignInManager>();

            // services
            //     .AddHostedService<IdentitySeederService>()
            //     .AddSingleton<IdentitySeeder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapBlazorHub();
                    endpoints.MapFallbackToPage("/_Host");
                    endpoints.MapControllerRoute(
                        name : "areas",
                        pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                    );
                }
            );
        }
    }
}
