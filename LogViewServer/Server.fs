namespace ULogViewServer
module Server = 
    open System.IO
    open FSharpTools

    let mutable send = fun (payload: obj) -> ()

    let changeTheme theme = send {| Method = Method.ChangeTheme; Theme = theme |} 

    type GetItems = {
        ReqId: int
        StartRange: int
        EndRange: int
    }

    type Message =
        | GetItems of GetItems

    let initialize (startServer: (Stream->unit)->(unit -> unit)-> byte array -> unit) = 
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
        ()

        send <- startServer onReceive onClose << Json.serializeToBuffer

    let loadLogFile logFilePath = 
        async {
            let! lines = FileOperations.loadLogFile logFilePath
            send {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            //startRefresh ()
            ()
        } |> Async.Start
       

        // TODO: in .vue: if current pos is at the end send refresh tail on otherwise send refresh tail off
        // TODO: Menu in Windows
        // TODO: OpenFile in Windows
        // TODO: respond default theme
        // TODO: Process indicator while loading file
