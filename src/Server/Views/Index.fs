module FunPizzaShop.Server.Views.Index
open Common
open Thoth.Json.Net
open Microsoft.AspNetCore.Http
open FunPizzaShop.ServerInterfaces.Query
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model

let extraThoth = Extra.empty |> Extra.withInt64 |> Extra.withDecimal

let view (env:_) (ctx:HttpContext) (dataLevel: int) = task{
    let query = env :> IQuery
    let! pizzas = query.Query<PizzaSpecial> (filter = Greater("BasePrice",1m), take = failwith "take 10 items") 
    let! toppings =  failwith "toppings query"
    let serializedToppings = Encode.Auto.toString (toppings, extra = failwith "extra decoters")
    let li = 
        pizzas 
        |> List.map (fun pizza -> 
        let serializedSpecial = 
            Encode.Auto.toString(pizza, extra = extraThoth)
        let serializedSpecial = System.Net.WebUtility.HtmlEncode serializedSpecial
        html $"""
            <li>
                <fps-pizza-item special='{serializedSpecial}'>
                    <div class="pizza-info" style="background-image: url('/assets/{failwith "Pizzaurl"}')">
                        <span class=title>{pizza.Name}</span>
                        {pizza.Description}
                        <span class=price>{failwith "Pizza formatted price"}</span>
                    </div>
                </fps-pizza-item>
            </li>
        """)
        |> failwith "contact"

    return
        html $""" 
            <fps-pizza-menu toppings='{serializedToppings}'>
                <ul class="pizza-cards">
                    {failwith "Pizza list"}
                </ul>
            </fps-pizza-menu>
            <div class="sidebar">
                <fps-side-bar></fps-side-bar>
            </div>
        """
}