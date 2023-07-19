module FunPizzaShop.Client.Checkout

open Elmish
open Elmish.HMR
open Lit
open Lit.Elmish
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core
open System
open Browser
open Elmish.Debug
open FsToolkit.ErrorHandling
open ElmishOrder
open FunPizzaShop.MVU
open FunPizzaShop.MVU.Checkout
open Thoth.Json
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.Shared.Constants
open CustomNavigation
open FunPizzaShop.Shared

let private hmr = HMR.createToken ()

module Server =
    open Fable.Remoting.Client
    let api: API.Order =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder (API.Route.builder None)
        |> Remoting.buildProxy<API.Order>
        
let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()
    | Order.GetPizzas ->
        let pizzaString:string = history.state |> unbox<string>
        let pizzas = Decode.Auto.fromString<Pizza list>(pizzaString,extra = extraEncoders)
        match pizzas with
        | Ok pizzas ->
            dispatch (SetPizzas pizzas) 
        | Error err -> console.error err


[<LitElement("fps-checkout")>]
let LitElement () =
    Hook.useHmr (hmr)
    let host, _ = LitElement.init (fun config -> 
        config.useShadowDom <- false
    )
    let program =
        Program.mkHiddenProgramWithOrderExecute (init) (update) (execute host)
#if DEBUG
        |> Program.withDebugger
        |> Program.withConsoleTrace
#endif
    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()