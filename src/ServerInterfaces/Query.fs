module FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model
open Akka.Streams.Dsl
open Akka.Streams
open FunPizzaShop.Shared.Model.Pizza

[<Interface>]
type IQuery =
    abstract Query<'t> : ?filter:Predicate * ?orderby:string * ?orderbydesc:string * ?thenby:string  * ?thenbydesc:string * ?take:int * ?skip:int -> list<'t> Async