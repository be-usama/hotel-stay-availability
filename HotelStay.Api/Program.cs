using HotelStay.Api.Endpoints;
using HotelStay.Api.Providers;
using HotelStay.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
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
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Stay Availability API v1");
    options.RoutePrefix = "swagger"; // Access at /swagger
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// Map endpoints
var searchService = app.Services.GetRequiredService<HotelSearchService>();
var reservationService = app.Services.GetRequiredService<ReservationService>();
app.MapHotelEndpoints(searchService, reservationService);

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

// Exposed so WebApplicationFactory<Program> can be used in integration tests.
public partial class Program { }