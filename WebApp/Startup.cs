using CK.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace WebApp
{
    public class Startup
    {
        readonly IConfiguration _configuration;
        readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            if (_env.IsDevelopment())
            {
                NormalizedPath dllPath = _configuration["StObjMap:Path"];
                if (!dllPath.IsEmpty)
                {
                    var solutionPath = new NormalizedPath(AppContext.BaseDirectory).RemoveLastPart(4);
                    dllPath = solutionPath.Combine(dllPath).AppendPart("CK.StObj.AutoAssembly.dll");
                    File.Copy(dllPath, Path.Combine(AppContext.BaseDirectory, "CK.StObj.AutoAssembly.dll"), overwrite: true);
                }
            }
            services.AddStObjMap("CK.StObj.AutoAssembly");
            services.AddCors();
            services.AddAmbientValues(_ => { });
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseRequestMonitor();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvcWithDefaultRoute();
        }
    }
}
