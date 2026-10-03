using System.Text.Json.Serialization;
using Application;
using Application.Users;
using Domain.Abstractions;
using EmailSender.Logger;
using FromToken;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using PasswordHasher.PBKDF2;
using Presentation;
using ServiceCollectorDI;
using Storage.EntityFramework;
using Storage.Validators;
using Swagger.CustomAPIDoc;
using TokenCreator.JWT;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEfStorage<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddPasswordHasher(builder.Configuration)
    .AddCustomSwaggerGen(isRequiredTargetClient: true)
    .AddTokenUser()
    .AddJwtTokenService(builder.Configuration)
    .AddServices(typeof(UserService).Assembly)
    .AddLoggingEmailSender()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddControllers(o => o.Conventions.Add(new ApiRouteConvention()))
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(AppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

EntityConfigurationValidator.Validate(typeof(Entity).Assembly, typeof(AppDbContext).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseApiDocumentation();

app.MapControllers();

app.Run();