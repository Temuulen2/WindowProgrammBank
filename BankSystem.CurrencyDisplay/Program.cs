using BankSystem.CurrencyDisplay.Components;

var builder = WebApplication.CreateBuilder(args);

// Razor компонентуудыг болон интерактив серверийн горимыг нэмнэ
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

// App.razor-г үндсэн компонент болгон бүртгэж, интерактив серверийн горимыг идэвхжүүлнэ
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
