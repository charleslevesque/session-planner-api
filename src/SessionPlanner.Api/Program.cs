using Microsoft.EntityFrameworkCore;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=SessionPlanner.db"));

var app = builder.Build();

app.MapPost("/sessions", async (AppDbContext db, Session session) =>
{
    db.Sessions.Add(session);
    await db.SaveChangesAsync();
    return Results.Created($"/sessions/{session.Id}", session);
});

app.MapGet("/sessions", async (AppDbContext db) =>
{
    return await db.Sessions.ToListAsync();    
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.Run();
