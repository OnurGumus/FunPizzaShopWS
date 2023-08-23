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

let random = System.Random.Shared

let actorProp (env:_) toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<obj>) =
    let config  = env :> IConfiguration
    let log = mailbox.UntypedContext.GetLogger()
    let mediatorS = retype mediator
    let sendToSagaStarter = SagaStarter.toSendMessage mediatorS mailbox.Self
    let rec set (state: State) =

        let apply (event: Event) (state:State) =
            log.Debug("Apply Message {@Event}, State: @{State}", event, state)
            match event with
            | LoginSucceeded(code) ->
                failwith "change state"
            | _ -> state
    
        actor {
            let! msg = mailbox.Receive()
            log.Debug("Message {MSG}, State: {@State}", box msg, state)
            match msg with
            | PersistentLifecycleEvent _
            | :? Persistence.SaveSnapshotSuccess
            | LifecycleEvent _ -> failwith "no state change"

            | SnapshotOffer(snapState: obj) -> return! snapState |> unbox<_> |> set

                // actor level events will come here
            | Persisted mailbox (:? Common.Event<Event> as event) ->
                let version = event.Version
                SagaStarter.publishEvent mailbox mediator event event.CorrelationId

                let state = {
                    (apply event.EventDetails state) with
                        Version = version
                }

                if (version >= 30L && version % 30L = 0L) then
                    return! state |> set <@> SaveSnapshot(state)
                else
                    return! failwith "set state"
                    
            | Recovering mailbox (:? Common.Event<Event> as event) ->
                return!
                    {
                        (apply event.EventDetails state) with
                            Version = event.Version
                    }
                    |> set

            | _ ->
                match msg with
                | :? Persistence.RecoveryCompleted -> return! failwith "recovery completed"

                | :? (Common.Command<Command>) as userMsg ->
                    let ci = userMsg.CorrelationId
                    let commandDetails = userMsg.CommandDetails
                    let v = state.Version

                    match commandDetails with
                    | (VefifyLogin incomingCode) ->
                        let verficiationEvent =
                            if mailbox.Pid.Contains("@" |> Uri.EscapeDataString) then
                                if incomingCode.IsNone then VerificationSucceeded
                                else
                                match state.Verification with
                                | Some(code) when code = incomingCode.Value -> VerificationSucceeded
                                | _ -> VerificationFailed
                            else
                                VerificationFailed

                        let verficiationOutcome =
                            toEvent ci (v + 1L) verficiationEvent |> sendToSagaStarter ci |> box |> Persist
                        return! verficiationOutcome

                    | Login ->
                        try
                            let verificationCode =
                               failwith "generate verification code"
                                |> forceValidate

                            let lastSlash = mailbox.Pid.LastIndexOf("/")

                            let email =
                                mailbox.Pid
                                    .Substring(lastSlash + 1)
                                    |> Uri.UnescapeDataString
                                    |> UserId.TryCreate
                                    |> forceValidate

                            let body =  
                                $"""Your verification code is <b>{failwith "verCode"}</b>"""
                                |> LongString.TryCreate |> forceValidate

                            let subject = "Verification Code" |> ShortString.TryCreate |> forceValidate
                            printfn "Sending verification code %A" verificationCode
                            //          (mailSender.SendVerificationMail email subject body

                            let e = LoginSucceeded( Some verificationCode)
                            return! toEvent ci (failwith "increment version") e |> sendToSagaStarter ci |> box |> Persist

                        with ex ->
                                log.Error(ex, "Error sending verification code")
                                let e2 = LoginFailed
                                return! toEvent ci (failwith "dont increment version") e2 |> box |> Persist
                | _ ->
                        log.Debug("Unhandled Message {@MSG}", box msg)
                        return Unhandled
        }
    set {
        Version = 0L
        Verification = None
    }

let init (env: _) toEvent (actorApi: IActor) =
    AkklingHelpers.entityFactoryFor actorApi.System shardResolver "User"
    <| propsPersist (actorProp env toEvent (typed actorApi.Mediator))
    <| false

let factory (env: _) toEvent actorApi entityId =
    (init env toEvent actorApi).RefFor (failwith "shard id") (failwith "entity")