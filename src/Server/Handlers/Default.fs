module FunPizzaShop.Server.Handlers.Default
open System.Threading.Tasks
open Giraffe
open Microsoft.AspNetCore.Http
open FunPizzaShop.Server.Views

let webApp (env:_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =

    let viewRoute view = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! lay = (layout ctx (view ctx))
                return! htmlString lay next ctx
            }

    let defaultRoute = 
            // fetch toppings and pizzas here and pass down to index
            viewRoute (Index.view env)

    choose [ 
        routeCi "/checkout" >=> failwith "checkout" 
        routeCi "/" >=> defaultRoute
    ]

let webAppWrapper (env:_) (layout: HttpContext -> (int -> Task<string>) -> string Task) =
    fun (next: HttpFunc) (context: HttpContext) -> task { 
        return! webApp env layout next context
     }