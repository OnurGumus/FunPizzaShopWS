module FunPizzaShop.Query.Projection

open FSharp.Data.Sql
open Serilog
open Akka.Persistence.Query
open Akka.Persistence.Query.Sql
open FSharp.Data.Sql.Common
open Akkling.Streams
open Akka.Streams
open Thoth.Json.Net
open Command.Serialization
open FunPizzaShop.Shared.Model.Authentication
open FunPizzaShop
open FunPizzaShop.Shared.Model.Pizza
open Command.Common
open FunPizzaShop.ServerInterfaces.Query


[<Literal>]
let resolutionPath = __SOURCE_DIRECTORY__ + @"/libs"

[<Literal>]
let schemaLocation = __SOURCE_DIRECTORY__ + @"/../Server/Database/Schema.sqlite"
#if DEBUG

[<Literal>]
let connectionString =
    @"Data Source=" + __SOURCE_DIRECTORY__ + @"/../Server/Database/FunPizzaShop.db;"

#else

[<Literal>]
let connectionString = @"Data Source=" + @"Database/FunPizzaShop.db;"

#endif

[<Literal>]
let connectionStringReal = @"Data Source=" + @"Database/FunPizzaShop.db;"

type Sql =
    SqlDataProvider<DatabaseProviderTypes.SQLITE, SQLiteLibrary=SQLiteLibrary.MicrosoftDataSqlite, 
    ConnectionString=connectionString, ResolutionPath=resolutionPath,
    ContextSchemaPath=schemaLocation,
    CaseSensitivityChange=CaseSensitivityChange.ORIGINAL>

let ctx = Sql.GetDataContext(connectionString)

let inline encode<'T> =
    Encode.Auto.generateEncoderCached<'T> (caseStrategy = CamelCase, extra = extraThoth)
    >> Encode.toString 0

let inline decode<'T> =
    Decode.Auto.generateDecoderCached<'T> (caseStrategy = CamelCase, extra = extraThoth)
    |> Decode.fromString

open FunPizzaShop.Command.Domain
let handleEvent (connectionString: string) (subQueue: ISourceQueue<_>) (envelop: EventEnvelope) =
    ()


open Command.Actor
let handleEventWrapper (connectionString: string) (actorApi:IActor) (subQueue: ISourceQueue<_>) (envelop: EventEnvelope) =
    try
        handleEvent connectionString subQueue envelop
    with ex ->
        Log.Error(ex, "Error during event handling")
        actorApi.System.Terminate().Wait()
        Log.CloseAndFlush()
        System.Environment.Exit(-1)

let readJournal system =
    PersistenceQuery
        .Get(system)
        .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)

let init (connectionString: string) (actorApi: IActor) =
    Log.Information("init query side")
    let offsetCount =
        query {
            for o in ctx.Main.Offsets do
                where (o.OffsetName = "Users")
                select o.OffsetCount
                exactlyOne
        }
    failwith "Not implemented"

    let source =
        (readJournal actorApi.System)
            .EventsByTag("default", Offset.Sequence(offsetCount))
            
    System.Threading.Thread.Sleep(100)

    Log.Information("Journal started")
    let subQueue = Source.queue OverflowStrategy.Fail 1024
    let subSink = (Sink.broadcastHub 1024)

    let runnableGraph = subQueue |> Source.toMat subSink Keep.both

    let queue, subRunnable = runnableGraph |> Graph.run (actorApi.Materializer)
    source
    |> Source.recover (fun ex ->
        Log.Error(ex, "Error during event reading pipeline")
        None)
    |> Source.runForEach actorApi.Materializer (handleEventWrapper connectionString actorApi queue)
    |> Async.StartAsTask
    |> ignore
    
    System.Threading.Thread.Sleep(1000)
    Log.Information("Projection init finished")
    subRunnable

