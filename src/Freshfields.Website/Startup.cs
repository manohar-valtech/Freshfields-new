using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.ContentApi.Core.Configuration;
using EPiServer.ContentApi.Core.DependencyInjection;
using Geta.Optimizely.Sitemaps;
using Stott.Optimizely.RobotsHandler.Configuration;
using Freshfields.Web.Infrastructure.Initialization;
using Freshfields.Headless.Infrastructure.DependencyInjection;

namespace Freshfields.Web;

public class Startup(IWebHostEnvironment webHostingEnvironment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(webHostingEnvironment.ContentRootPath, "App_Data"));

        services
            .AddCmsAspNetIdentity<ApplicationUser>()
            .AddCms()
            .AddAdminUserRegistration()
            .AddEmbeddedLocalization<Program>()
            .ConfigureForExternalTemplates()
            .Configure<DisplayOptions>(options => options
                    .Add("full", "Full", "u-md-sizeFull", string.Empty, "epi-icon__layout--full")
                    .Add("wide", "Wide", "u-md-size2of3", string.Empty, "epi -icon__layout--two-thirds")
                    .Add("half", "Half", "u-md-size1of2", string.Empty, "epi-icon__layout--half")
                    .Add("narrow", "Narrow", "u-md-size1of3", string.Empty, "epi-icon__layout--one-third"));

        services.AddContentDeliveryApi();

        services.ConfigureForContentDeliveryClient();

        services.Configure<ContentApiOptions>(options =>
        {
            options.ForceAbsolute = false;
        });

        services.AddTinyMceConfigurations();

        services.AddRobotsHandler();

        services.AddSitemaps(x =>
        {
            x.EnableLanguageDropDownInAdmin = true;
            x.EnableRealtimeCaching = true;
            x.EnableRealtimeSitemap = false;
        });

        services.AddHeadless();

        services.AddNodeJs(options =>
        {
            if (webHostingEnvironment.IsDevelopment())
            {
                options.LaunchCommand = "npm run dev";
                options.WorkingDirectory = "./ClientApp/";
                options.RedirectOutput = false;
            }
            else
            {
                var port = 4000;

                options.DestinationPort = port;
                options.LaunchCommand = "node ./server/index.mjs";
                // If server contains secrets, these files needs to be served from elsewhere.
                // We can leverage the static file middleware by serving them from here.
                options.WorkingDirectory = "./wwwroot/";
                options.EnvironmentVariables = new Dictionary<string, string>
                {
                    { "PORT", $"{port}" },
                    { "NEXT_PUBLIC_API_URL", "http://localhost:80/api/episerver/v3.0/" },
                    { "NEXT_PUBLIC_WEBSITE_URL", "http://www.example.com/" }
                };
            }
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
            endpoints.MapContent();
            endpoints.MapNodeJs();
        });
    }
}
