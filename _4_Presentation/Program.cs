using Application;
using Application.Users;
using EmailSender.Logger;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using PasswordHasher.PBKDF2;
using Presentation;
using ServiceCollectorDI;
using Storage.EntityFramework;
using Swagger.CustomAPIDoc;
using TokenCreator.JWT;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddEfStorage<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddPasswordHasher(builder.Configuration)
    .AddCustomSwaggerGen()
    .AddJwtTokenService(builder.Configuration)
    .AddServices(typeof(UserService).Assembly)
    .AddLoggingEmailSender()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddControllers(o => o.Conventions.Add(new ApiRouteConvention()));


builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(AppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();