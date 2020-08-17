namespace ULogViewServer
open System
open FSharpTools

type GetItems = {
    ReqId: int
    StartRange: int
    EndRange: int
}

type SetRefreshMode = {
    Value: bool
}

type Message =
    | GetItems of GetItems
    | SetRefreshMode of SetRefreshMode

type Session() = 
    // TODO: LateInit
    let mutable send: Action<obj> = Action<obj>(ignore)
    let mutable formatMilliseconds = false

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            let items = FileOperations.getLines getItems.StartRange getItems.EndRange
            send.Invoke {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            return! loop ()
        }
        loop ()  
    )    

    let mutable refreshMode = true

    let rec startRefresh () = 
        async {
            let lines = FileOperations.refresh ()
            if lines <> 0 then
                send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            do! Async.Sleep 100
            if refreshMode then startRefresh ()
        } |> Async.Start        

    let onReceive msg =
        match msg with
        | GetItems getItems -> getItemsQueue.Post getItems   
        | SetRefreshMode setRefreshMode -> 
            refreshMode <- setRefreshMode.Value
            if refreshMode then startRefresh () 

    member this.OnReceive payload =
        let msg = Json.deserializeStream<Message> payload
        onReceive msg

    member this.OnReceive payload =
        let msg = Json.deserialize<Message> payload 
        onReceive msg

    member this.OnClose () =
        ()

    member this.FormatMilliseconds
        with get () = formatMilliseconds
        and set value = 
            formatMilliseconds <- value
            FileOperations.setFormatMilliseconds value

    member this.Initialize(sendToSet: Action<obj>) =
        send <- sendToSet 

    member this.LoadLogFile logFilePath = 
        async {
            let! lines = FileOperations.loadLogFile logFilePath
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            startRefresh ()
            ()
        } |> Async.Start

    