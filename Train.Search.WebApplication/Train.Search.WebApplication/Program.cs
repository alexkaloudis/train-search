using Train.Search.WebApplication.Infrastructure.DtoManipulators;
using Train.Search.WebApplication.Infrastructure.ExternalHttpServices;
using Train.Search.WebApplication.Infrastructure.Models.Configuration;
using Train.Search.WebApplication.Infrastructure.Search;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

//Allow the frontend requests
builder.Services.AddCors(options => {
    options.AddPolicy(name: myAllowSpecificOrigins,
        policy =>
        {
            policy
                .WithOrigins(
                    "http://127.0.0.1:4200",
                    "http://localhost:4200",
                    "https://localhost:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RailUris>(
    builder.Configuration.GetSection(
        key: nameof(RailUris)));
builder.Services.AddScoped<IRailHttpClient, RailHttpClient>();
builder.Services.AddScoped<OpenSearchService>();
builder.Services.AddScoped<ApacheLuceneSearchService>();
builder.Services.AddScoped<PagedResultManipulator>();
builder.Services.AddHttpClient("irail-client", client =>
{
    client.BaseAddress = new Uri("https://api.irail.be/v1/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(myAllowSpecificOrigins);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();