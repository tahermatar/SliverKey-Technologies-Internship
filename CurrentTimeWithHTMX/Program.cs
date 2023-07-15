var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/datetime", () =>
{
    return DateTime.Now.ToString();
});
app.Run();
