using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Diagnostics;

namespace Ice.Configuration;

public static class WebApplicationExtension
{
    public static WebApplication UseIce(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseCustomExceptionHandler();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Top}/{action=Index}/{id?}");

        app.MapServerSentEvents("/sse-endpoint");

        return app;
    }
    
    private static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (exceptionHandlerPathFeature?.Error is not null)
                {
                    SentrySdk.CaptureException(exceptionHandlerPathFeature.Error);
                }

                // 従来のエラーページにリダイレクト
                context.Response.Redirect("/Error/500");
                await Task.CompletedTask;
            });
        });
    }
}