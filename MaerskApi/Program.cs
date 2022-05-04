using Microsoft.EntityFrameworkCore;
using MaerskApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddControllers(o => o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter()));
builder.Services.AddControllers();
builder.Services.AddDbContext<MaerskContext>(opt =>
    opt.UseInMemoryDatabase("MaerskList"));
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });

    app.UseExceptionHandler(
           new ExceptionHandlerOptions()
           {
               AllowStatusCode404Response = true, // important!
               ExceptionHandlingPath = "/error"
           }
       ); app.UseHsts();
    app.UseHttpLogging();


}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
