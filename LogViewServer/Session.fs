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
    let mutable send: Action<obj> = Action<obj>(ignore)

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            let items = FileOperations.getLines getItems.StartRange getItems.EndRange
            send.Invoke {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            return! loop ()
        }
        loop ()  
    )    

    let onReceive msg =
        match msg with
        | GetItems getItems -> getItemsQueue.Post getItems   

    let rec startRefresh () = 
        async {
            let lines = FileOperations.refresh ()
            if lines <> 0 then
                send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            do! Async.Sleep 100
            startRefresh ()
        } |> Async.Start        

    member this.OnReceive payload =
        let msg = Json.deserializeStream<Message> payload
        onReceive msg

    member this.OnReceive payload =
        let msg = Json.deserialize<Message> payload 
        onReceive msg

    member this.OnClose () =
        ()

    member this.Initialize(sendToSet: Action<obj>) =
        send <- sendToSet 

    member this.LoadLogFile logFilePath = 
        async {
            let! lines = FileOperations.loadLogFile logFilePath
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            startRefresh ()
            ()
        } |> Async.Start

        // TODO: in .vue: if current pos is at the end send refresh tail on otherwise send refresh tail off
    