open System
open System.IO

open WebWindowNetCore
open WebWindow

if Environment.CurrentDirectory.Contains "netcoreapp" then
    Environment.CurrentDirectory <- Path.Combine (Environment.CurrentDirectory, "../../../../../../")

#if DEBUG
let uri = Globals.debugUrl + "/"
#else
let uri = sprintf "http://localhost:%d/" Globals.port
#endif

let callback (text: string) = ()

let openFile file = 
    // TODO:
    // HeaderBar.SetSubtitle (headerBar, file)
    LogView.loadLogFile file

let dropFiles text =
    let files = text |> String.splitChar '|'
    openFile files.[0]

let server = Webserver.start ()

let configuration = { 
    defaultConfiguration () with
        Title = "LogView👌"
        Url = uri
        DebuggingEnabled = true
        Organization = "URiegel"
        Application = "LogView"
        SaveWindowSettings = true
        FullScreenEnabled = true
        OnEvent = callback
        DropFiles = dropFiles
}

[<EntryPoint>]
[<STAThread>]
let main argv =
    initialize configuration

    openFile @"C:\ProgramData\caesar\Proxy\log\CaesarProxy.log"

    execute () |> ignore
    server.stop ()
    0
    // TODO: Refresh mode refreshes indefinitly