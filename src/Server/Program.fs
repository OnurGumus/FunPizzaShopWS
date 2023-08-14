module FunPizzaShop.Server.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.SerilogExtensions
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Serilog
open Hocon.Extensions.Configuration
open ThrottlingTroll
open FunPizzaShop.Server.Views
open FunPizzaShop.Server.Handlers.Default
open HTTP
open System.Globalization


failwith "Set current thread culture to invariant"


bootstrapLogger()

type Self = Self
 
let errorHandler (ex: Exception) (ctx: HttpContext) =
    Log.Error(ex, "Error Handler")
    match ex with
    | :? System.Text.Json.JsonException -> clearResponse >=>  failwith "set status code" >=> text ex.Message
    | _ -> clearResponse >=> failwith "set status code" >=> text ex.Message

let configureCors (builder: CorsPolicyBuilder) =
        #if DEBUG
        builder
            .WithOrigins("http://localhost:5010", "https://localhost:5011")
            .AllowAnyMethod()
            .AllowAnyHeader()
        |> ignore
        #else 
            ()
        #endif


let configureApp (app: IApplicationBuilder, appEnv) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    let isDevelopment = env.IsDevelopment()
   
   
    let app = if isDevelopment then app else app.UseResponseCompression()

    app
        .UseDefaultFiles()
        .UseAuthentication()
        .UseAuthorization()
    |> ignore
    failwith "use LogUserNameMiddleware and headerMiddleware"



    let layout ctx = Layout.view ctx (appEnv) (env.IsDevelopment())

    let webApp  =
            webAppWrapper appEnv layout

    let sConfig = Serilog.configure errorHandler 
    let handler = SerilogAdapter.Enable(webApp, sConfig)

    (match isDevelopment with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles(staticFileOptions)
        .UseWebSockets() 
        .UseGiraffe(handler)
    failwith "use throttling throll"

let configureServices (services: IServiceCollection) =
    services
        .AddAuthorization()
        .AddResponseCompression(fun options -> options.EnableForHttps <- true)
        .AddCors()
        .AddGiraffe()
        .AddAntiforgery()
        .AddApplicationInsightsTelemetry()
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(
            CookieAuthenticationDefaults.AuthenticationScheme,
            fun options ->
                failwith "add SlidingExpiration"
                options.ExpireTimeSpan <- TimeSpan.FromDays(7)
        )
    |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

let host appEnv args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")
    let host = 
        Host
            .CreateDefaultBuilder(args)
            .UseSerilog(Serilog.configureMiddleware)
            .ConfigureWebHostDefaults(fun webHostBuilder ->
                webHostBuilder
    #if !DEBUG
                    .UseEnvironment(Environments.Production)
    #else
                    .UseEnvironment(Environments.Development)
    #endif
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> (fun builder -> configureApp (builder, appEnv)))
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                |> ignore)
            .Build()
    host


[<EntryPoint>]
let main args =
    let configBuilder =
        ConfigurationBuilder()
            .AddUserSecrets<Self>()
            .AddHoconFile("config.hocon")
            .AddHoconFile("secrets.hocon", true)
            .AddEnvironmentVariables()
    
    let config = failwith "build config"

    let mutable ret = 0
    let appEnv = obj()
    try
        try
            (host appEnv args).Run()
        with ex -> 
            Log.Fatal(ex, "Host terminated unexpectedly")
            ret <- -1
    finally
        failwith "Log CloseAndFlush"
    ret