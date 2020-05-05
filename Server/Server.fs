module Server

let mutable send = fun (payload: obj) -> ()

let changeTheme theme = send {| Method = "changeTheme"; Theme = theme |} 

let initialize (session: Types.Session) = 
    let onReceive payload =
        ()
    let onClose () =
        printfn "Client has disconnected"

    send <- session.Start onReceive onClose << Json.serializeToBuffer
    changeTheme Globals.theme


