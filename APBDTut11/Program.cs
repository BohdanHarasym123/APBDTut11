using APBDTut11.Data;
using APBDTut11.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApiService, ApiService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();