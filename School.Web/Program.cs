using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School.Data;
using School.Data.Contracts.Repositories;
using School.Data.Repositories;
using School.Domain;
using School.Services.Reports;
using School.Services.Security;
using School.Services.Services;
using School.Web.Components;
using School.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SchoolDb")));

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ISchoolClassRepository, SchoolClassRepository>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademicTermRepository, AcademicTermRepository>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISchoolClassService, SchoolClassService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAcademicTermService, AcademicTermService>();
builder.Services.AddScoped<ITermReportService, TermReportService>();

builder.Services.AddScoped<AdminGuard>();
builder.Services.AddScoped<RoleRefreshCookieEvents>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.EventsType = typeof(RoleRefreshCookieEvents);
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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

static bool IsLocalUrl(string? url) =>
    !string.IsNullOrEmpty(url)
    && url[0] == '/'
    && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));

app.MapPost("/Account/Login", async (HttpContext httpContext, IUserService userService, [FromForm] string login, [FromForm] string password, [FromForm] string? returnUrl) =>
{
    var user = await userService.GetByLoginAsync(login.Trim());
    if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
    {
        var failureUrl = IsLocalUrl(returnUrl)
            ? $"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl!)}"
            : "/login?error=1";
        return Results.Redirect(failureUrl);
    }

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, UserPrincipalFactory.Create(user));

    return Results.Redirect(IsLocalUrl(returnUrl) ? returnUrl! : "/");
});

app.MapPost("/Account/Register", async (HttpContext httpContext, IUserService userService, ILogger<Program> logger, [FromForm] string login, [FromForm] string password) =>
{
    login = login.Trim();

    if (login.Length == 0 || login.Length > 256)
    {
        return Results.Redirect("/register?error=3");
    }

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
    var role = allUsers.Count == 0 ? Roles.Admin : Roles.Teacher;

    var newUser = new User
    {
        Login = login,
        PasswordHash = PasswordHasher.Hash(password),
        Role = role
    };

    try
    {
        await userService.AddAsync(newUser);
    }
    catch (ArgumentException ex)
    {
        logger.LogInformation("Registration rejected for {Login}: {Reason}", login, ex.Message);
        return Results.Redirect("/register?error=1");
    }

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, UserPrincipalFactory.Create(newUser));

    return Results.Redirect("/");
});

app.MapPost("/Account/Logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapGet("/report/term.csv", async (int termId, int classId, ITermReportService reportService) =>
{
    var report = await reportService.BuildAsync(termId, classId);
    if (report == null)
    {
        return Results.NotFound();
    }

    return Results.File(TermReportCsv.Build(report), "text/csv; charset=utf-8", TermReportCsv.BuildFileName(report));
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();