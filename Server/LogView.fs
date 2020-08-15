module LogView
open Model

type GetItems = {
    ReqId: int
    StartRange: int
    EndRange: int
}

type Message =
    | GetItems of GetItems

let mutable send = fun (payload: obj) -> ()

let changeTheme theme = send {| Method = Method.ChangeTheme; Theme = theme |} 

let initialize (session: Types.Session) = 

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            let items = FileOperations.getLines getItems.StartRange getItems.EndRange
            send {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            return! loop ()
        }
        loop ()  
    )

    let onReceive payload =
        match Json.deserializeStream<Message> payload with
        | GetItems getItems -> getItemsQueue.Post getItems   
    let onClose () =
        printfn "Client has disconnected"

    send <- session.Start onReceive onClose << Json.serializeToBuffer
    changeTheme Globals.theme

let rec startRefresh () = 
    async {
        let lines = FileOperations.refresh ()
        if lines <> 0 then
            send {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
        do! Async.Sleep 100
        startRefresh ()
    } |> Async.Start


let loadLogFile logFilePath = 
    async {
        let! lines = FileOperations.loadLogFile logFilePath
        send {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
        startRefresh ()
    } |> Async.Start
    ()

    // TODO: in .vue: if current pos is at the end send refresh tail on otherwise send refresh tail off
    // TODO: Menu in Windows
    // TODO: OpenFile in Windows
    // TODO: respond default theme
    // TODO: Process indicator while loading file

