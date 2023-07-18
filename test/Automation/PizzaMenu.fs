module FunPizzaShop.Automation.PizzaMenu
open FunPizzaShop
open Microsoft.Playwright
open TickSpec
open Microsoft.Extensions.Hosting
open type Microsoft.Playwright.Assertions

[<When>]
let ``I get the main menu``  (context:IBrowserContext)= 
        (task {
                let! page = context.NewPageAsync()
                let! _ = page.GotoAsync("http://localhost:5010")
                return page
        }).Result
        


[<Then>]
let ``pizza items should be fetched`` (page:IPage)= 
        (task{
                let! pizzaItems = page.QuerySelectorAllAsync("fps-pizza-item")
                if pizzaItems.Count = 0 then
                        failwith "No pizza items found"
                else
                        return ()
        }).Wait()