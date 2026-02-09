using BioTime.Services.Areas;
using BioTime.Services.Employees;
using BioTime.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// BioTime
builder.Services.Configure<BioTimeSettings>(builder.Configuration.GetSection(BioTimeSettings.Section));
builder.Services.AddHttpClient("BioTime", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BioTime:BaseUrl"]!);
});
builder.Services.AddSingleton<IBioTimeService, BioTimeService>();
builder.Services.AddSingleton<IAreaService, AreaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
