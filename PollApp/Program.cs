using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PollApp.Data;
using PollApp.Models;
using PollApp.Services;
using PollApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IVoteService, VoteService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Poll}/{action=Index}/{id?}");
app.MapHub<ResultsHub>("/resultsHub");

try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();  // This line throws if packages mismatch—fix packages first
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        var adminEmail = "admin@example.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        if (!context.Polls.Any())
        {
            var poll = new Poll { Title = "Sample Poll", Description = "Test description", StartDate = DateTime.UtcNow, IsActive = true };
            context.Polls.Add(poll);
            await context.SaveChangesAsync();
            context.Options.AddRange(
                new Option { Text = "Yes", PollId = poll.Id },
                new Option { Text = "No", PollId = poll.Id }
            );
            await context.SaveChangesAsync();
        }
    }
}
catch (Exception ex)
{
    // Log the error for debugging (in production, use ILogger)
    Console.WriteLine($"Seeder error: {ex.Message}");
    throw;  // Re-throw to see full stack trace
}

app.Run();