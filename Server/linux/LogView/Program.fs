open System
open System.IO

if Environment.CurrentDirectory.Contains "netcoreapp" then
    Environment.CurrentDirectory <- Path.Combine (Environment.CurrentDirectory, "../../../../../../")

let server = Webserver.start ()
Mainwindow.run ()
server.stop ()