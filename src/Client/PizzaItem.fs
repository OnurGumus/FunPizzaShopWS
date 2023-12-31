module FunPizzaShop.Client.PizzaItem

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
open Browser.Types
open FunPizzaShop.MVU.PizzaItem
open Thoth.Json
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.Shared.Constants

let private hmr = HMR.createToken ()

let rec execute (host: LitElement) order (dispatch: Msg -> unit) =
    match order with
    | Order.NoOrder -> ()

[<HookComponent>]
let view (host:LitElement) (model:Model) dispatch =
    printf "%A" model
    Hook.useEffectOnce (fun () -> 
        host?addEventListener("click", (fun (e: MouseEvent) -> 
            host.dispatchCustomEvent (Events.PizzaSelected ,model.PizzaSpecial, true, true,true)
        )) |> ignore
    )
    Lit.nothing
    
[<LitElement("fps-pizza-item")>]
let LitElement () =
#if DEBUG
    Hook.useHmr hmr
#endif
 
    let split (str: string): PizzaSpecial option =
        let res = Decode.Auto.fromString<PizzaSpecial>(str, extra = extraEncoders)
        match res with
            | Ok x -> Some x
            | Error x -> console.error(x); Option.None

    let host, prop = LitElement.init (fun config -> 
   
        config.useShadowDom <- false
        config.props <- {|
            special = Prop.Of( Option.None , attribute="special", fromAttribute = split)
        |}
    
    )
    
    let program =
        Program.mkHiddenProgramWithOrderExecute 
            (init (prop.special.Value.Value)) (update) (execute host)
    #if DEBUG
            |> Program.withDebugger
            |> Program.withConsoleTrace
    #endif

    let model, dispatch = Hook.useElmish program
    view host model dispatch

let register () = ()