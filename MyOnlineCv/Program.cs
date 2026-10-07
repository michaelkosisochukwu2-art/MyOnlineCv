using MyOnlineCv.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages and API Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Configure Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Swagger Documentation Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MyOnlineCv API", Version = "v1" });
});

var app = builder.Build();

// Auto-create/migrate Azure SQL Database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // Option A: If using EF Core Migrations (Recommended)
        dbContext.Database.Migrate();

        // Option B: If not using migrations (Keep if you prefer simple auto-creation)
        // dbContext.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the Azure SQL Database.");
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable Swagger UI (Accessible at /swagger)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyOnlineCv API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// 1. Serve standard static files from wwwroot (CSS, JS, favicons)
app.UseStaticFiles();

// 2. Ensure external persistent directory exists (C:\home\site\gallery_uploads)
var persistentUploadsPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "gallery_uploads"));
if (!Directory.Exists(persistentUploadsPath))
{
    Directory.CreateDirectory(persistentUploadsPath);
}

// 3. Serve uploaded files from C:\home\site\gallery_uploads at the path /gallery_uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(persistentUploadsPath),
    RequestPath = "/gallery_uploads"
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();