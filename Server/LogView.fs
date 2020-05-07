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
            let payload = {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            send (payload :> obj)
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

let loadLogFile logFilePath = 
    async {
        let! lines = FileOperations.loadLogFile logFilePath
        send {| Method = "itemsSource"; Count = lines; IndexToSelect = lines - 1 |} 
    } |> Async.Start
    ()

    // TODO: respond default theme
    // TODO: getItems
    // TODO: Process indicator while loading file

