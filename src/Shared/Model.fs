module rec FunPizzaShop.Shared.Model

open System
open Fable.Validation
open FsToolkit.ErrorHandling
open Thoth.Json


let inline forceValidate e = 
    match e with
    | Ok x -> x
    | Error e -> failwith e

type Predicate =
    | Greater of string * IComparable
    | GreaterOrEqual of string * IComparable
    | Smaller of string * IComparable
    | SmallerOrEqual of string * IComparable
    | Equal of string * obj
    | NotEqual of string * obj
    | And of Predicate * Predicate
    | Or of Predicate * Predicate
    | Not of Predicate


type Version =
    | Version of int64
    member this.Value:int64 = let (Version v) = this in v
    member this.Zero = Version 0L

type ShortStringError =
    | EmptyString
    | TooLongString

type ShortString = 
    private | ShortString of string
    member this.Value = let (ShortString s) = this in s
    static member Create (s:string) = 
        single(fun t ->
            t.TestOne s
            |> t.MinLen 1 ShortStringError.EmptyString
            |> t.MaxLen 255 ShortStringError.TooLongString
            |> t.Map ShortString
            |> t.End
        )