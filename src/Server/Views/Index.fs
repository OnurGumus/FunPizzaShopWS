module FunPizzaShop.Server.Views.Index
open Common
open Thoth.Json.Net
open Microsoft.AspNetCore.Http
open FunPizzaShop.ServerInterfaces.Query
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model

let view (env:_) (ctx:HttpContext) (dataLevel: int) = task{
    let query = env :> IQuery
    let! pizzas = query.Query<PizzaSpecial> (filter = Greater("BasePrice",1m), take = 10) 
    let! toppings = query.Query<Topping> ()
    let li = 
        pizzas 
        |> List.map (fun pizza -> 
        html $"""
            <li>
                <fps-pizza-item>
                    <div class="pizza-info" style="background-image: url('/assets/{pizza.ImageUrl}')">
                        <span class=title>{pizza.Name}</span>
                        {pizza.Description}
                        <span class=price>{pizza.FormattedBasePrice}</span>
                    </div>
                </fps-pizza-item>
            </li>
        """)
        |> String.concat "\r\n"

    return
        html $""" 
            <fps-pizza-menu>
                <ul class="pizza-cards">
                    {li}
                </ul>
            </fps-pizza-menu>
            <div class="sidebar">
                <fps-side-bar></fps-side-bar>
            </div>
        """
}