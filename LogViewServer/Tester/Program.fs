open System
open System.IO
open Server
open Websocket
open Model

if Environment.CurrentDirectory.Contains "netcoreapp" then
    Environment.CurrentDirectory <- Path.Combine (Environment.CurrentDirectory, "../../../../../../")

let initialize (session: Types.Session) = 
    Server.initialize session.Start
    Server.loadLogFile "/home/uwe/LogTest/test.log"
    
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