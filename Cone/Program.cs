using Cone.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddConeConfiguration();

var app = builder.Build();
app.UseCone();
app.Run();