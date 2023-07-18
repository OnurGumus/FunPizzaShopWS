module FunPizzaShop.Automation.PizzaMenu
open FunPizzaShop
open Microsoft.Playwright
open TickSpec
open Microsoft.Extensions.Hosting
open type Microsoft.Playwright.Assertions

[<When>]
let ``I get the main menu``  (context:IBrowserContext)= 
        ()


[<Then>]
let ``pizza items should be fetched`` ()= ()