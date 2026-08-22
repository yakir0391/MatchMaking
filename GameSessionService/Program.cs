using GameSessionService.Consumers;
using GameSessionService.Data;
using GameSessionService.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;
using Shared.Infrastructure.Messaging.RabbitMQ.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<GameService>();
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHostedService<MatchFoundConsumer>();
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddDbContext<GameSessionDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("GameSessionDatabase")));
builder.Services.AddScoped<IGameService, GameService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<GameSessionDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.Run();
