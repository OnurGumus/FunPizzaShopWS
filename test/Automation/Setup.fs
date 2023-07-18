module FunPizzaShop.Automation.Setup

open FunPizzaShop
open Microsoft.Playwright
open TickSpec
open Microsoft.Extensions.Hosting
open System.Threading.Tasks
open System
open Microsoft.Extensions.Configuration
open Hocon.Extensions.Configuration
open System.IO
open FunPizzaShop.Server
let configBuilder =
    ConfigurationBuilder()
        .AddHoconFile("test-config.hocon")
        .AddEnvironmentVariables()


let config = configBuilder.Build()

Directory.SetCurrentDirectory("/workspaces/FunPizzaShop/src/Server")

let appEnv = Environments.AppEnv(config)

let host = App.host appEnv [||]

host.Start()

let playwright = Playwright.CreateAsync().Result
let browser = playwright.Chromium.LaunchAsync().Result

