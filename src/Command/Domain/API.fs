module FunPizzaShop.Command.Domain.API

open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Command
open Common
open Akka.Cluster.Sharding
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open NodaTime
open Actor
open Akkling.Cluster.Sharding
open Microsoft.Extensions.Configuration

let sagaCheck (env:_) toEvent actorApi (clock: IClock) (o: obj) =
    match o with
    | :? (Event<Order.Event>) as e ->
        match e with
        | { EventDetails = Order.OrderPlaced _  } ->
            [ (OrderSaga.factory env toEvent actorApi clock, id |> Some |> PrefixConversion) ]
        | _ -> []
    | :? (Event<Delivery.Event>) as e ->
        match e with 
        | { EventDetails = Delivery.DeliveryStarted _  } ->
            [ (DeliverySaga.factory env toEvent actorApi clock, id |> Some |> PrefixConversion ) ]
        | _ -> []
    | _ -> []


[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract UserFactory: string -> IEntityRef<obj>
    abstract OrderFactory: string -> IEntityRef<obj>
    
let api (env: _) (clock: IClock) (actorApi: IActor) =
    let toEvent ci = Common.toEvent clock ci
    SagaStarter.init actorApi.System actorApi.Mediator (sagaCheck env toEvent actorApi clock)
    User.init env toEvent actorApi |> sprintf "User initialized: %A" |> Log.Debug
    Order.init env toEvent actorApi |> sprintf "Order initialized: %A" |> Log.Debug
    OrderSaga.init env toEvent actorApi clock |> sprintf "OrderSaga initialized: %A" |> Log.Debug
    DeliverySaga.init env toEvent actorApi clock |> sprintf "DeliverySaga initialized: %A" |> Log.Debug
    System.Threading.Thread.Sleep(1000)
    { new IDomain with
        member _.Clock = clock
        member _.ActorApi = actorApi
        member _.UserFactory entityId =
            User.factory env toEvent actorApi entityId
        member _.OrderFactory entityId =
            Order.factory env toEvent actorApi entityId
    }