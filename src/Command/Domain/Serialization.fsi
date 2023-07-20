module Command.Serialization

open Command
open Akkling
open Akka.Actor
open Akka.Serialization
open System.Text
open NodaTime
open Thoth.Json.Net
open System.Runtime.Serialization
open Serilog
open System
open FunPizzaShop.Command.Domain

module DefaultEncode =
    val instant: instant: Instant -> JsonValue

module DefeaultDecode =
    val instant: Decoder<Instant>

val extraThoth: ExtraCoders
val userMessageEncode: Encoder<Common.Event<User.Event>>
val userMessageDecode: Decoder<Common.Event<User.Event>>
/// State encoding
val userStateEncode: Encoder<User.State>
val userStateDecode: Decoder<User.State>

type ThothSerializer =
    new: system: ExtendedActorSystem -> ThothSerializer
    inherit SerializerWithStringManifest
    override Identifier: int
    override ToBinary: obj: obj -> byte array
    override Manifest: o: obj -> string
    override FromBinary: bytes: byte array * manifest: string -> obj
