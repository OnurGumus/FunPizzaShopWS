module FunPizzaShop.Server.Environments


open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.ServerInterfaces.Query
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model
open FunPizzaShop.ServerInterfaces.Command
open FunPizzaShop.Shared.Command.Authentication
open FunPizzaShop.Shared.Command.Pizza


[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) as self =

    let mutable commandApi =
        lazy(FunPizzaShop.Command.API.api self NodaTime.SystemClock.Instance)
    
    let mutable queryApi =
            lazy(FunPizzaShop.Query.API.api config commandApi.Value.ActorApi)
          

    do 
        DB.init config

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)


    interface IAuthentication with
        member _.Login: Login = 
            commandApi.Value.Login
            
        member _.Logout: Logout = 
            fun () -> 
                async { 
                    return  Ok()
                }
        member _.Verify: Verify = 
            commandApi.Value.Verify

    interface IPizza with
        member _.Order: OrderPizza = 
            failwith "what here"
            
    interface IQuery with
        member _.Query(?filter, ?orderby,?orderbydesc, ?thenby, ?thenbydesc, ?take, ?skip) =
            queryApi.Value.Query(?filter = filter, ?orderby = orderby, ?orderbydesc = orderbydesc, ?thenby = thenby, ?thenbydesc = thenbydesc,  ?take = take, ?skip = skip)
    
        member _.Subscribe(cb) =  failwith "how to subscribe"
    

    member _.Reset() = ()

    member _.Init() = 
        if commandApi.Value = Unchecked.defaultof<_> 
                ||  queryApi.Value = Unchecked.defaultof<_>   then
            failwith "AppEnv not initialized"
        