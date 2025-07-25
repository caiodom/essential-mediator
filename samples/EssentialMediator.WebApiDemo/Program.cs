using EssentialMediator.Extensions;
using EssentialMediator.WebApiDemo.Data;
using EssentialMediator.WebApiDemo.Middleware;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "EssentialMediator WebAPI Demo", 
        Version = "v1",
        Description = "Demo Test",
        Contact = new() { Name = "EssentialMediator", Url = new Uri("https://github.com/caiodom/essential-mediator") }
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    c.EnableAnnotations();
    c.UseInlineDefinitionsForEnums();
});

builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemoryDatabase("DemoDatabase"));


builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Information);
});


builder.Services.AddEssentialMediator(Assembly.GetExecutingAssembly())
    .AddAllBuiltInBehaviors(slowRequestThresholdMs: 500); // 500ms threshold for performance monitoring

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EssentialMediator WebAPI Demo v1");
    c.RoutePrefix = string.Empty; 
    c.DocumentTitle = "EssentialMediator WebAPI Demo";
    c.DefaultModelsExpandDepth(-1);
});

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await SeedData(dbContext);
}

app.Run();

static async Task SeedData(DemoDbContext context)
{
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new EssentialMediator.WebApiDemo.Models.User { Id = 1, Name = "John Smith", Email = "john@email.com", CreatedAt = DateTime.UtcNow },
            new EssentialMediator.WebApiDemo.Models.User { Id = 2, Name = "Jane Doe", Email = "jane@email.com", CreatedAt = DateTime.UtcNow },
            new EssentialMediator.WebApiDemo.Models.User { Id = 3, Name = "Bob Johnson", Email = "bob@email.com", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
    }
}
