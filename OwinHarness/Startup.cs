using System;
using System.Net;

using Microsoft.Owin;
using Microsoft.Owin.Logging;

using Owin;
using OwinHarness;

using Serilog;
using Serilog.Extras.MSOwin;

using ILogger = Serilog.ILogger;
using LoggerFactory = Serilog.Extras.MSOwin.LoggerFactory;

[assembly: OwinStartup(typeof(Startup))]

namespace OwinHarness
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ILogger logger =
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(outputTemplate: "{Timestamp} [{Level}] ({RequestId}) {Message}{NewLine}{Exception}")
                    .CreateLogger();

            logger.Information("OWIN Pipline initilizing");

            // This causes Serilog to capture logging done with to Microsoft's built-in OWIN logging infrastructure
            app.SetLoggerFactory(new LoggerFactory(logger));

            // This assigns a request context which is available to logging done by middleware following
            // this registration.  For this to work you need to remember to enrich from the log context
            app.UseSerilogRequestContext();

            app.UseErrorPage();

            app.Map("/error", builder =>
            {
                builder.Run(context =>
                {
                    throw new ApplicationException("Note how this is logged by the error page.");
                });
            });

            app.Use(async (context, next) =>
            {
                await next();
                logger.Information(
                    "Request for {path} completed with status code {statusCode}",
                    context.Request.Path,
                    context.Response.StatusCode);
            });

            app.Run(async context =>
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/hello")))
                {
                    var path = context.Request.Path.ToString();
                    var name = path.Substring(path.IndexOf('/', 1) + 1);

                    logger.Information("Saying Hello to {name}", name);
                    await context.Response.WriteAsync("Hello " + name);
                }
                else
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
            });

            logger.Information("OWIN Pipline initialization complete");
        }
    }
}