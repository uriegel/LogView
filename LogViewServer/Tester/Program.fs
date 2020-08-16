open System
open System.IO
open Server
open Websocket
open ULogViewServer    
open FSharpTools

if Environment.CurrentDirectory.Contains "netcoreapp" then
    Environment.CurrentDirectory <- Path.Combine (Environment.CurrentDirectory, "../../../../../../")

let session = Session ()

let initialize (socketSession: Types.Session) = 
    let onReceive stream = session.OnReceive stream
    let onClose = session.OnClose
    let send = socketSession.Start onReceive onClose << Json.serializeToBuffer
    let func = System.Action<obj>(send)
    session.Initialize func
    session.LoadLogFile "/home/uwe/LogTest/test.log"
    
let start () = 
    let requests = [ 
        useWebsocket "/logview" initialize
    ]

    let configuration = Configuration.create {
        Configuration.createEmpty() with 
            Port = 9866
            AllowOrigins = Some [| "http://localhost:8080" |]
            Requests = requests
    }
    let server = create configuration 
    server.start ()
    server

let stop (server: Server) = server.stop ()

let server = start ()
Console.ReadLine () |> ignore
server.stop ()