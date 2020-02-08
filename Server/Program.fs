let configuration = Configuration.create {
    Configuration.createEmpty() with 
        WebRoot = "/home/uwe/projects/UwebServer/webroot" 
        Port = 20000
        AllowOrigins = Some [| "http://localhost:8080" |]
        favicon = "Uwe.jpg"
}

try 
    let server = Server.create configuration 
    server.start ()
    stdin.ReadLine() |> ignore
    server.stop ()
with
    | ex -> printfn "Fehler: %O" ex