module FunPizzaShop.Server.Environments


open System
open Microsoft.Extensions.Configuration
open FunPizzaShop.ServerInterfaces.Query

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) =
 
    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)


    interface IQuery with
        member _.Query(?filter, ?orderby,?orderbydesc, ?thenby, ?thenbydesc, ?take, ?skip) =
            failwith "Not implemented"

    member _.Reset()  = ()
