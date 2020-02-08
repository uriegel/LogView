module Main
open Session
open Request
open Requests

type LineCount = { LineCount: int }
type PathInput = { Path: string }
type LinesInput = { LineIndexes: int array }
type LinesReponse = { Lines: string seq }

let asyncRequest (requestSession: RequestSession) = 
    async {
        let method = requestSession.url.Substring(requestSession.url.LastIndexOf('/') + 1) 
        match method with
        | "loadLogFile" -> 
            let input = asyncGetJson<PathInput> requestSession.requestData
            loadLogFile input.Path
            let res = { LineCount = getLineCount () }
            do! requestSession.asyncSendJson (res :> obj)
            return true
        | "getLines" ->
            let input = asyncGetJson<LinesInput> requestSession.requestData
            let res = { Lines = getLines input.LineIndexes }
            do! requestSession.asyncSendJson (res :> obj)
            return true
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
