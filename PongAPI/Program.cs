using PongAPI.Hubs;

namespace PongAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddSignalR();
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy", builder =>
				{
					builder.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:5051/").AllowCredentials();
				});
			});
			builder.Services.AddRazorPages();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
			}
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthorization();
			app.UseCors("CorsPolicy");
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<PongHub>("/pongHub");
			});

			app.MapRazorPages();

			app.Run();
		}
	}
}
