module FunPizzaShop.MVU.LoginStore
open ElmishStore
open FunPizzaShop.Shared.Model.Authentication

type Model = { UserId: UserId option }

type Msg = 
    | LoggedIn of UserId
    | LoggedOut 

[<RequireQualifiedAccessAttribute>]
type Order =
    | NoOrder

let init () = { UserId = None }, Order.NoOrder

let update (msg: Msg) (model: Model) =
    match msg with
    | LoggedIn userId -> { model with UserId = failwith "UserId" },Order.NoOrder
    | LoggedOut -> { model with UserId = failwith "UserId" },Order.NoOrder

let dispose _ = ()