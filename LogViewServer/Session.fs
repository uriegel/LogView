namespace ULogViewServer
open System
open FSharpTools

type GetItems = {
    ReqId: int
    StartRange: int
    EndRange: int
}

type Message =
    | GetItems of GetItems


type Session() = 
    // TODO: LateInit
    let mutable send: Func<obj, Unit> = Func<obj, Unit>(ignore)

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            let items = FileOperations.getLines getItems.StartRange getItems.EndRange
            send.Invoke {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            return! loop ()
        }
        loop ()  
    )    

    member this.OnReceive payload =
        match Json.deserializeStream<Message> payload with
        | GetItems getItems -> getItemsQueue.Post getItems   

    member this.OnClose () =
        printfn "Client has disconnected"

    member this.Initialize(sendToSet: Func<obj, Unit>) =
        send <- sendToSet 

    member this.LoadLogFile logFilePath = 
        async {
            let! lines = FileOperations.loadLogFile logFilePath
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            //startRefresh ()
            ()
        } |> Async.Start

    // let changeTheme theme = send {| Method = Method.ChangeTheme; Theme = theme |} 

        // TODO: in .vue: if current pos is at the end send refresh tail on otherwise send refresh tail off
        // TODO: Menu in Windows
        // TODO: OpenFile in Windows
        // TODO: respond default theme
        // TODO: Process indicator while loading file
