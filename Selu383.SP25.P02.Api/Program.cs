using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Security;

namespace Selu383.SP25.P02.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1) Add EF Core
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DataContext")
                    ?? throw new InvalidOperationException("Connection string 'DataContext' not found.")));

            // 2) Add controllers
            builder.Services.AddControllers();

            // 3) Add cookie-based authentication
            builder.Services.AddAuthentication("CookieScheme")
                .AddScheme<AuthenticationSchemeOptions, CookieAuthHandler>("CookieScheme", _ => {});
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Migrate + seed 
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                await db.Database.MigrateAsync();
                SeedTheaters.Initialize(scope.ServiceProvider);
            }

            // If your tests call HTTP (not HTTPS), comment out or remove:
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}