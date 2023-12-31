module FunPizzaShop.Command.API

open Command
open Common
open Serilog
open Actor
open NodaTime
open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.Command.Domain.API
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza
open FunPizzaShop.Command.Domain
open FunPizzaShop.Shared.Model.Pizza

val createCommandSubscription:
    domainApi: IDomain ->
    factory: (string -> #Akkling.Cluster.Sharding.IEntityRef<obj>) ->
    id: string ->
    command: 'b ->
    filter: ('c -> bool) ->
        Async<Event<'c>>

module User =
    open FunPizzaShop.Shared.Model.Authentication

    val login:
        createSubs: (string -> User.Command -> (User.Event -> bool) -> Async<Event<User.Event>>) ->
        userId: UserId ->
            Async<Result<unit, LoginError list>>

    val verify:
        createSubs: (string -> User.Command -> (User.Event -> bool) -> Async<Event<User.Event>>) ->
        userId: UserId * verCode: VerificationCode option ->
            Async<Result<unit, VerificationError list>>

[<Interface>]
type IAPI =
    abstract ActorApi: IActor
    abstract Login: Login
    abstract Verify: Verify

val api: env: IConfiguration -> clock: IClock -> IAPI
