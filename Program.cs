using Microsoft.EntityFrameworkCore;
using SvendeApi.Data;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SvendeApi.Mappers;
using SvendeApi.Interface;
using SvendeApi.Services;
using SvendeApi.Hubs;
using SvendeApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return uri.Host == "localhost" && uri.Port == 5173;
                return false;
            });
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserMapper>();
    cfg.AddProfile<RoleMapper>();
    cfg.AddProfile<AuthMapper>();
    cfg.AddProfile<PostMapper>();
    cfg.AddProfile<LikeMapper>();
    cfg.AddProfile<FollowMapper>();
    cfg.AddProfile<CommenMapper>();
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IRoleService, RoleService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(120)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"JWT failed: {ctx.Exception}");
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine($"JWT challenge: {ctx.Error} {ctx.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<FeedHub>("/hubs/feed");

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     var defaultRoles = new[] { "Admin", "User", "Manager" };
//     foreach (var name in defaultRoles)
//     {
//         if (!db.Roles.Any(r => r.RoleName == name))
//         {
//             db.Roles.Add(new RoleModel { RoleId = Guid.NewGuid(), RoleName = name });
//         }
//     }
//     db.SaveChanges();
// }

app.Run();


