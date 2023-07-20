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

val sagaCheck: env: 'a -> toEvent: 'b -> actorApi: 'c -> clock: IClock -> o: obj -> 'd list

[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract UserFactory: string -> IEntityRef<obj>

val api: env: IConfiguration -> clock: IClock -> actorApi: IActor -> IDomain
