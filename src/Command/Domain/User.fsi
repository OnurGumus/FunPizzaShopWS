module FunPizzaShop.Command.Domain.User

open Command
open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Common
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open Actor
open Microsoft.Extensions.Configuration
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop.Shared.Model
open Akka.Logger.Serilog
open Akka.Event
open FunPizzaShop.ServerInterfaces.Command

type Command =
    | Login
    | VefifyLogin of VerificationCode option

type Event =
    | LoginSucceeded of VerificationCode option
    | LoginFailed
    | VerificationFailed
    | VerificationSucceeded

type State =
    { Verification: VerificationCode option
      Version: int64 }

    interface IDefaultTag

val random: Random

val actorProp:
    env: IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'a>) ->
    mediator: IActorRef<Publish> ->
    mailbox: Eventsourced<obj> ->
        Effect<obj>

val init:
    env: IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'a>) ->
    actorApi: IActor ->
        Cluster.Sharding.EntityFac<obj>

val factory:
    env: IConfiguration ->
    toEvent: (string -> int64 -> Event -> Event<'a>) ->
    actorApi: IActor ->
    entityId: string ->
        Cluster.Sharding.IEntityRef<obj>
