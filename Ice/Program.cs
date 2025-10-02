using Ice.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddIceConfiguration();

var app = builder.Build();
app.UseIce();
app.Run();