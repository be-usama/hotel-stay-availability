using HotelStay.Api.Endpoints;
using HotelStay.Api.Providers;
using HotelStay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
    {
        p.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Register hotel providers
builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();

// Register services
builder.Services.AddSingleton(sp =>
{
    var providers = sp.GetRequiredService<IEnumerable<IHotelProvider>>().ToArray();
    return new HotelSearchService(providers);
});
builder.Services.AddSingleton<ReservationService>();

var app = builder.Build();

// Configure pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map endpoints
var searchService = app.Services.GetRequiredService<HotelSearchService>();
var reservationService = app.Services.GetRequiredService<ReservationService>();
app.MapHotelEndpoints(searchService, reservationService);

app.Run();