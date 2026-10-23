var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку контроллеров и представлений (MVC)
builder.Services.AddControllersWithViews();

// Регистрация HttpClient с актуальным портом запущенного API
builder.Services.AddHttpClient("UniversityApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7201/");
});

var app = builder.Build();

// Настройка конвейера обработки HTTP-запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Настройка стандартного маршрута MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Courses}/{action=Index}/{id?}");

app.Run();