using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School.Data;
using School.Data.Repositories;
using School.Domain;
using School.Services.Security;
using School.Services.Services;
using School.Web.Components;
using School.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDb")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISchoolClassService, SchoolClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<AdminGuard>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

static bool IsLocalUrl(string? url) =>
    !string.IsNullOrEmpty(url)
    && url[0] == '/'
    && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));

app.MapPost("/Account/Login", async (HttpContext httpContext, IUserService userService, [FromForm] string login, [FromForm] string password, [FromForm] string? returnUrl) =>
{
    var user = await userService.GetByLoginAsync(login);
    if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
    {
        var failureUrl = IsLocalUrl(returnUrl)
            ? $"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl!)}"
            : "/login?error=1";
        return Results.Redirect(failureUrl);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, user.Login),
        new(ClaimTypes.Role, user.Role)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect(IsLocalUrl(returnUrl) ? returnUrl! : "/");
});

app.MapPost("/Account/Register", async (HttpContext httpContext, IUserService userService, [FromForm] string login, [FromForm] string password) =>
{
    if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
    {
        return Results.Redirect("/register?error=2");
    }

    var existing = await userService.GetByLoginAsync(login);
    if (existing != null)
    {
        return Results.Redirect("/register?error=1");
    }

    var allUsers = await userService.GetAllAsync();
    var role = allUsers.Count == 0 ? "Admin" : "Teacher";

    var newUser = new User
    {
        Login = login,
        PasswordHash = PasswordHasher.Hash(password),
        Role = role
    };

    await userService.AddAsync(newUser);

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, newUser.Login),
        new(ClaimTypes.Role, newUser.Role)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect("/");
});

app.MapPost("/Account/Logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();