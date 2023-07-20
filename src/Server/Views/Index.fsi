module FunPizzaShop.Server.Views.Index

open Common
open Thoth.Json.Net
open Microsoft.AspNetCore.Http
open FunPizzaShop.ServerInterfaces.Query
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model

val extraThoth: ExtraCoders
val view: env: IQuery -> ctx: HttpContext -> dataLevel: int -> System.Threading.Tasks.Task<string>
