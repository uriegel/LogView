module Main
open Session
open Request
open Requests

type LineCount = { LineCount: int }
type PathInput = { Path: string }
type LinesInput = { LineIndexes: int array }

let asyncRequest (requestSession: RequestSession) = 
    async {
        let query = requestSession.query.Value
        match query.Method with
        | "loadLogFile" -> 
            match query.Query "path" with
            | Some path -> 
                loadLogFile path
                let res = { LineCount = getLineCount () }
                do! requestSession.asyncSendJson (res :> obj)
                return true
            | None -> 
                failwith "no path"
                return false
        | "getLines" ->
            match (query.Query "startRange", query.Query "endRange") with
            | (Some startRange, Some endRange) -> 
                let res = getLines (int startRange) (int endRange) 
                do! requestSession.asyncSendJson (res :> obj)
                return true
            | _ -> 
                failwith "no path"
                return false
        | _ -> return false
    }

let configuration = Configuration.create {
    Configuration.createEmpty() with 
        WebRoot = "/home/uwe/projects/UwebServer/webroot" 
        Port = 20000
        asyncRequest = asyncRequest
        AllowOrigins = Some [| "http://localhost:8080" |]
        favicon = "Uwe.jpg"
}

type SubTest = {
    Name: string
    Id: int
}

type Test = {
    Path: string
    SubTest: SubTest
    Vielleicht: string option
    Nichts: string option
}

[<EntryPoint>]
let main argv =
    try 
        let server = Server.create configuration 
        server.start ()
        stdin.ReadLine() |> ignore
        server.stop ()
        0
    with
        | ex -> 
            printfn "Exception: %O" ex
            9
