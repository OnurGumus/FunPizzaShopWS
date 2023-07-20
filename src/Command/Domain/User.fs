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
        actor {
            let! msg = mailbox.Receive()
            log.Debug("Message {MSG}, State: {@State}", box msg, state)
            match msg with
                // actor level events will come here
            | _ ->
                match msg with
                | :? (Common.Command<Command>) as userMsg ->
                    let ci = userMsg.CorrelationId
                    let commandDetails = userMsg.CommandDetails
                    let v = state.Version

                    match commandDetails with
                    | Login ->
                        try
                            let verificationCode =
                                VerificationCode.TryCreate(random.Next(100000, 999999).ToString())
                                |> forceValidate

                            let lastSlash = mailbox.Pid.LastIndexOf("/")

                            let email =
                                mailbox.Pid
                                    .Substring(lastSlash + 1)
                                    |> Uri.UnescapeDataString
                                    |> UserId.TryCreate
                                    |> forceValidate

                            let body =  
                                $"Your verification code is <b>{verificationCode.Value}</b>" 
                                |> LongString.TryCreate |> forceValidate

                            let subject = "Verification Code" |> ShortString.TryCreate |> forceValidate
                            //          (mailSender.SendVerificationMail email subject body

                            let e = LoginSucceeded( Some verificationCode)
                            return! toEvent ci (v + 1L) e |> box |> Persist

                        with ex ->
                                log.Error(ex, "Error sending verification code")
                                let e2 = LoginFailed
                                return! toEvent ci v e2 |> box |> Persist
                    | _ ->
                        log.Debug("Unhandled Message {@MSG}", box msg)
                        return Unhandled
        }
    failwith "not implemented"