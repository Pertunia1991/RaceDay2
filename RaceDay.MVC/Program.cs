
using RaceDay.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC services
builder.Services.AddControllersWithViews();

// Configure session storage
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure connection to RaceDay2.API
builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    var apiAddress = builder.Configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiAddress))
    {
        throw new InvalidOperationException(
            "RaceDay API BaseUrl is missing from appsettings.json.");
    }

    client.BaseAddress = new Uri(apiAddress);
});

// Register RaceDay API service
builder.Services.AddScoped<RaceDayApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authorization and controllers
app.UseSession();

app.UseAuthorization();

// Default MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
