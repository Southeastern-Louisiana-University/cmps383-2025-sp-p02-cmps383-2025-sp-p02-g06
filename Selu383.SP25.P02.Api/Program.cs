﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Users;
using Selu383.SP25.P02.Api.Features.Roles;

namespace Selu383.SP25.P02.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ? Add Database Context
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext") ??
                    throw new InvalidOperationException("Connection string 'DataContext' not found.")));

            // ? Add Identity Authentication
            builder.Services.AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthorization();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            builder.Services.AddControllers();

            // ✅ Add Swagger Services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ? ENSURE DATABASE IS MIGRATED & SEEDED AT STARTUP
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<DataContext>();

                await db.Database.MigrateAsync();  // ? Ensure DB is up to date
                await SeedUsersAndRoles.EnsureSeededAsync(services);  // ? Ensure Users & Roles Are Seeded
                SeedTheaters.Initialize(scope.ServiceProvider); // ? Ensure Theaters Are Seeded
            }

            app.UseHttpsRedirection();

            // ✅ Enable Swagger UI in Development Mode
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }




            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(x =>
                {
                    x.MapControllers();
                });
            app.UseStaticFiles();
            if (app.Environment.IsDevelopment())
            {
                app.UseSpa(x =>
                {
                    x.UseProxyToSpaDevelopmentServer("http://localhost:5173");
                });
            }
            else
            {
                app.MapFallbackToFile("/index.html");
            }
            app.Run();
        }
    }
}
