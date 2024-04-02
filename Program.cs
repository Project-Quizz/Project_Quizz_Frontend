using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project_Quizz_Frontend.Data;
using Project_Quizz_Frontend.Services; // Ensure you include the namespace for your QuizApiService

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
	.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Register HttpClient and QuizApiService

//API Key implementation
//var apiKey = builder.Configuration["ApiKey"];
//builder.Services.AddTransient(_ => new ApiKeyHandler(apiKey));
//builder.Services.AddHttpClient<QuizApiService>()
//    .AddHttpMessageHandler<ApiKeyHandler>();

builder.Services.AddHttpClient<QuizApiService>(); // This adds IHttpClientFactory to be used for creating HttpClient instances
builder.Services.AddScoped<QuizApiService>(); // Register your QuizApiService



// Session support
builder.Services.AddSession();

var app = builder.Build();

// Automatische Migration beim Start
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();