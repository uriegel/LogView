module Main
open System.Runtime.Serialization
open Session
open Requests

[<DataContract>]
type Command = {
    [<DataMember>]
    mutable cmd: string

    [<DataMember>]
    mutable requestId: string

    [<DataMember>]
    mutable count: int64
}

[<DataContract>]
type PathInput = {
    [<DataMember>]
    mutable path: string
}

let asyncRequest (requestSession: RequestSession) = 
    async {



// public static T Deserialize<T>(Stream s)
// {
//     using (StreamReader reader = new StreamReader(s))
//     using (JsonTextReader jsonReader = new JsonTextReader(reader))
//     {
//         JsonSerializer ser = new JsonSerializer();
//         return ser.Deserialize<T>(jsonReader);
//     }
// }

        let method = requestSession.url.Substring(requestSession.url.LastIndexOf('/') + 1) 
        match method with
        | "loadLogFile" -> 
            let input = requestSession.asyncGetJson typedefof<PathInput> :?> PathInput
            loadLogFile input.path
            let command = {
                cmd = "Kommando"
                requestId = "RekwestEidie"
                count = 2234335L
            }
            do! requestSession.asyncSendJson (command :> obj)            
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
        let test = { Path= "Der Pfad"; SubTest = { Name = "Der Name"; Id = 343 }; Vielleicht = Some "vielleicht"; Nichts = None }

        use ms = new System.IO.MemoryStream()
        Json.serializeStreamWithOptions ms test 
        ms.Capacity <- int ms.Length
        let buffer = ms.GetBuffer ()
        let res = System.Text.Encoding.UTF8.GetString (buffer)

        let buffer = System.Text.Encoding.UTF8.GetBytes (res)
        use ms = new System.IO.MemoryStream (buffer)
        let ress = Json.deserializeStreamWithOptions<Test> ms 

        let server = Server.create configuration 
        server.start ()
        stdin.ReadLine() |> ignore
        server.stop ()
        0
    with
        | ex -> 
            printfn "Exception: %O" ex
            9
