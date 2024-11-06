using Microsoft.AspNetCore.Identity;
using WebCityEvents.Data;
using WebCityEvents.Models;

public class DbInitializationMiddleware
{
    private readonly RequestDelegate _next;

    public DbInitializationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EventContext dbContext)
    {
        if (await dbContext.IsDatabaseEmptyAsync())
        {
            await DbInitializer.InitializeAsync(userManager, roleManager, dbContext);
        }

        await _next(context);
    }
}