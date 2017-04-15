using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using BestMeetingSpot.Services;

namespace BestMeetingSpot
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
			services.AddMvc();
			services.AddSwaggerGen();
			services.ConfigureSwaggerGen(options =>
			{
				options.SingleApiVersion(new Info
				{
					Version = "v1",
					Title = "BestMeetingSpot",
					Description = "This API is an abstraction layer above GoogleMapsAPI in order to find the best place to meet.",
					TermsOfService = "None",
					Contact = new Contact() { Name = "Juan Cruz Montes", Email = "jcmontes95@gmail.com" }
				});
			});
			services.AddScoped<IGoogleMapsService>(x => new GoogleMapsService());
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
			app.UseStaticFiles();
			app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUi();
        }
    }
}
