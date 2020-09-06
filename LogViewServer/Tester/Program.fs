open System
open System.IO
open Server
open Websocket
open ULogViewServer    
open FSharpTools

if Environment.CurrentDirectory.Contains "netcoreapp" then
    Environment.CurrentDirectory <- Path.Combine (Environment.CurrentDirectory, "../../../../../../")

let mutable sessionHolder: Session Option = None

let initialize (socketSession: Types.Session) = 
    //let session = Session ("/home/uwe/LogTest/test.log", false, true) //session.LoadLogFile "/home/uwe/LogTest/testm.log"
    let session = Session ("/home/uwe/LogTest/CaesarProxy.log", false, false)
    //let session = Session ("/home/uwe/LogTest/CaesarProxy2.log", true, false)
    //let session = Session ("/home/uwe/LogTest/klein.log", true, false)
    //let session = Session (@"C:\ProgramData\caesar\Proxy\log\CaesarProxy.log", true, false)
    //let session = Session (@"C:\users\uwe\desktop\CaesarWebServer.log", false, false)
    
    sessionHolder <- Some session 
    let onReceive (stream: Stream) = session.OnReceive stream
    let onClose = session.OnClose
    let send = socketSession.Start onReceive onClose << Json.serializeToBuffer
    let func = Action<obj>(send)
    session.Initialize func

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