global using Microsoft.AspNetCore.Authorization;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using System.ComponentModel.DataAnnotations;
global using System.Security.Claims;
global using Newtonsoft.Json;
global using AutoMapper;
global using Hangfire;
global using Polish_Clips.Models;
global using Polish_Clips.Data;
global using Polish_Clips.Dtos.Game;
global using Polish_Clips.Dtos.User;
global using Polish_Clips.Dtos.Clip;
global using Polish_Clips.Dtos.Report;
global using Polish_Clips.Dtos.Helpers;
global using Polish_Clips.Dtos.Comment;
global using Polish_Clips.Services.TwitchApiService;
global using Polish_Clips.Services.ClipService;
global using Polish_Clips.Services.GameService;
global using Polish_Clips.Services.CommentService;
global using Polish_Clips.Services.ReportService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using HangfireBasicAuthenticationFilter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
    
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = """Standard Authorization header using the Bearer scheme. Example: "bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Policy1",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://polish-clips.vercel.app")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITwitchApiService, TwitchApiService>();
builder.Services.AddScoped<IClipService, ClipService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
            .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddHttpContextAccessor();

builder.Services.AddHangfire((sp, config) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    config.UseSqlServerStorage(connectionString);
});

var _configuration = builder.Configuration;

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("Policy1");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
        {
                new HangfireCustomBasicAuthenticationFilter{
                    User = _configuration.GetSection("HangfireSettings:UserName").Value,
                    Pass = _configuration.GetSection("HangfireSettings:Password").Value
                }
            }
});

//RecurringJob.AddOrUpdate<TwitchApiService>("AddClipsJob", (x) =>
//x.AddClipsByStreamers(), "0 * * * *");//every full hour

//RecurringJob.AddOrUpdate<TwitchApiService>("AddBroadcastersJob", (x) =>
//x.AddBroadcasters(), "0 */3 * * *");//every 3 full hours

RecurringJob.AddOrUpdate<TwitchApiService>("AddKeepAppAliveJob", (x) =>
x.KeepAppAlive(), "*/3 * * * *");//every 3 minutes

app.Run();