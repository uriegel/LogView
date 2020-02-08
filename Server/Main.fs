module Main
open System.Runtime.Serialization
open Session
open Requests

// TODO: use module Json from FSharpTools

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

type OptionConverter() =
    inherit Newtonsoft.Json.JsonConverter()
    
    override x.CanConvert(objectType) = 
        objectType.IsGenericType && objectType.GetGenericTypeDefinition() = typedefof<option<_>>

    override x.WriteJson(writer, value, serializer) =
        let value = 
            match value with 
            | null -> null
            | _ -> 
                let _, fields = Microsoft.FSharp.Reflection.FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, value)

    override x.ReadJson(reader, objectType, existingValue, serializer) =        
        let innerType = objectType.GetGenericArguments().[0]
        let innerType = 
            if innerType.IsValueType then (typedefof<System.Nullable<_>>).MakeGenericType([|innerType|])
            else innerType        
        let value = serializer.Deserialize(reader, innerType)
        let cases = Microsoft.FSharp.Reflection.FSharpType.GetUnionCases(objectType)
        if value = null then 
            Microsoft.FSharp.Reflection.FSharpValue.MakeUnion(cases.[0], [||])
        else 
            Microsoft.FSharp.Reflection.FSharpValue.MakeUnion(cases.[1], [|value|])

let seri a =

    use ms = new System.IO.MemoryStream()
    use writer = new System.IO.StreamWriter (ms)
    use jsonWriter = new Newtonsoft.Json.JsonTextWriter (writer)
    let ser = Newtonsoft.Json.JsonSerializer ()
    ser.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()
    ser.DefaultValueHandling <- Newtonsoft.Json.DefaultValueHandling.Ignore
    ser.Serialize (jsonWriter, a)
    jsonWriter.Flush()
    ms.Capacity  <- int ms.Length
    let buffer = ms.GetBuffer ()
    System.Text.Encoding.UTF8.GetString (buffer)

let seriWithOptions a =

    use ms = new System.IO.MemoryStream()
    use writer = new System.IO.StreamWriter (ms)
    use jsonWriter = new Newtonsoft.Json.JsonTextWriter (writer)
    let ser = Newtonsoft.Json.JsonSerializer ()
    ser.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()
    ser.DefaultValueHandling <- Newtonsoft.Json.DefaultValueHandling.Ignore

    ser.Converters.Add (OptionConverter ())

    ser.Serialize (jsonWriter, a)
    jsonWriter.Flush()
    ms.Capacity  <- int ms.Length
    let buffer = ms.GetBuffer ()
    System.Text.Encoding.UTF8.GetString (buffer)

let deseri<'T> (text: string) =
    let buffer = System.Text.Encoding.UTF8.GetBytes (text)
    use ms = new System.IO.MemoryStream (buffer)
    use reader = new System.IO.StreamReader (ms)
    use jsonReader = new Newtonsoft.Json.JsonTextReader (reader)
    let ser = Newtonsoft.Json.JsonSerializer ()
    ser.DefaultValueHandling <- Newtonsoft.Json.DefaultValueHandling.Ignore
    ser.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()
    ser.Deserialize<'T> (jsonReader)

let deseriWithOptions<'T> (text: string) =
    let buffer = System.Text.Encoding.UTF8.GetBytes (text)
    use ms = new System.IO.MemoryStream (buffer)
    use reader = new System.IO.StreamReader (ms)
    use jsonReader = new Newtonsoft.Json.JsonTextReader (reader)
    let ser = Newtonsoft.Json.JsonSerializer ()
    ser.DefaultValueHandling <- Newtonsoft.Json.DefaultValueHandling.Ignore
    ser.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver ()

    ser.Converters.Add (OptionConverter ())
    
    ser.Deserialize<'T> (jsonReader)

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

        let affe = seri test
        let zurück = deseri<Test> affe

        let affe2 = seriWithOptions test
        let zurück = deseriWithOptions<Test> affe2


        let server = Server.create configuration 
        server.start ()
        stdin.ReadLine() |> ignore
        server.stop ()
        0
    with
        | ex -> 
            printfn "Exception: %O" ex
            9
