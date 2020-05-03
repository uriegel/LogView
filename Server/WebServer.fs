module Webserver
open System.IO
open Websocket
open Server
open System.Text 
open Session

let start = 
    let requests = [ 
//        useWebsocket "/commander" Commander.initialize
        Static.useStatic (Path.Combine (Directory.GetCurrentDirectory (), "dist")) "/" 
    ]

    let configuration = Configuration.create {
        Configuration.createEmpty() with 
            Port = Globals.port
            AllowOrigins = Some [| Globals.debugUrl |]
            Requests = requests
    }
    let server = create configuration 
    server.start ()
    server

let stop (server: Server) = server.stop ()