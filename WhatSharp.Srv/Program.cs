using Microsoft.EntityFrameworkCore;
using WhatSharp.Srv.Data;
using WhatSharp.Srv.Hubs;
using WhatSharp.Srv.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enregistre UserServiceSrv comme service injectable
builder.Services.AddScoped<UserServiceSrv>();

builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddCors(o => o.AddPolicy("signalr", p =>
{
    p.AllowAnyHeader().AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true);
}));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseCors("signalr");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
