using EmailSenderLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewAgePOS.Models;
using Microsoft.AspNetCore.Http;
using ChannelAdvisorLibrary;
using NewAgePOSLibrary.Databases;
using NewAgePOSLibrary.Data;
using SkuVaultLibrary;
using NewAgePOSModels.Securities;
using System.Globalization;

namespace NewAgePOS
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
      services.AddDbContextPool<LogRegContext>(options =>
      {
        if (Secrets.DBIsLocal)
          options.UseSqlServer(Configuration.GetConnectionString("EmployeeDBConnection"));
        else
          options.UseSqlServer($"Server=posacct;Database=posacct;User Id={ Secrets.DBUser };Password={ Secrets.DBPassword }");
      });

      services.AddIdentity<EmployeeModel, IdentityRole>(options => 
        options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<LogRegContext>()
        .AddDefaultTokenProviders();

      services.AddRazorPages(options =>
      {
        options.Conventions.AuthorizeFolder("/Product");
        options.Conventions.AuthorizeFolder("/Report");
        options.Conventions.AuthorizeFolder("/Sale");
        options.Conventions.AuthorizePage("/Account/Register", ClaimTypeEnum.Admin.ToString());
      });

      services.ConfigureApplicationCookie(options =>
      {
        options.AccessDeniedPath = new PathString("/AccessDenied");
      });

      services.AddSession();

      services.AddTransient<IEmailSender, EmailSender>();
      services.AddTransient<ISQLData, SQLData>();
      services.AddTransient<ISQLDataAccess, SQLDataAccess>();

      services.AddScoped<IChannelAdvisor, ChannelAdvisor>();
      services.AddScoped<ISkuVault, SkuVault>();

      services.AddAuthorization(options =>
      {
        options.AddPolicy(ClaimTypeEnum.Admin.ToString(), policy => policy
          .RequireClaim(ClaimTypeEnum.Admin.ToString(), "true"));
      });

      CultureInfo ci = new CultureInfo("en-US");
      ci.NumberFormat.CurrencyNegativePattern = 0;
      CultureInfo.DefaultThreadCurrentCulture = ci;
      CultureInfo.DefaultThreadCurrentUICulture = ci;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, LogRegContext context)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      //app.UseHttpsRedirection();
      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseSession();

      context.Database.Migrate();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapRazorPages();
      });
    }
  }
}
