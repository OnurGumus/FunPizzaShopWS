module FunPizzaShop.Server.Environments


open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.ServerInterfaces.Command
open FunPizzaShop.Shared.Command.Authentication

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv =
    new: config: IConfiguration -> AppEnv
    interface IConfiguration
    interface IAuthentication
    interface IQuery
    member Reset: unit -> unit
