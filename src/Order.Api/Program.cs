using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Order.Api;
using Savorboard.CAP.InMemoryMessageQueue;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddControllers();
builder.Services.AddCap(options =>
{
    options.UseInMemoryStorage();
    // options.UseRabbitMQ("localhost");
    options.UseInMemoryMessageQueue();
    options.UseDashboard();
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.MapControllers();
