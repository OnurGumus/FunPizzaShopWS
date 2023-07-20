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

type State = {
    Verification: VerificationCode option
    Version: int64
} with
    interface IDefaultTag


let actorProp (env:_) toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<obj>) =
    let config  = env :> IConfiguration
    let log = mailbox.UntypedContext.GetLogger()
    let mediatorS = retype mediator
    let sendToSagaStarter = SagaStarter.toSendMessage mediatorS mailbox.Self
    let rec set (state: State) =
        actor {
            let! msg = mailbox.Receive()
            log.Debug("Message {MSG}, State: {@State}", box msg, state)

        }
    failwith "not implemented"