using Microsoft.EntityFrameworkCore;
using testingStuff.data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<DbDataContext>(opt =>
    opt.UseInMemoryDatabase("Users"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


// SWAGGER THINGS


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// MORE SWAGGER
// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test V1");
        c.RoutePrefix = string.Empty;
    });
}


app.UseHttpsRedirection();

/*
app.UseDefaultFiles();
app.UseStaticFiles();
*/

app.UseAuthorization();

app.MapControllers();

app.Run();
