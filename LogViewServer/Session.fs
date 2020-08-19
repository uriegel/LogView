namespace ULogViewServer
open System
open System.Text
open FSharpTools

type GetItems = {
    ReqId: int
    StartRange: int
    EndRange: int
}

type SetRefreshMode = {
    Value: bool
}

type SetRestriction = {
    Restriction: string option
}

type Message =
    | GetItems of GetItems
    | SetRefreshMode of SetRefreshMode
    | SetRestriction of SetRestriction

type Session(logFilePath: string, formatMilliseconds: bool, utf8: bool) = 
    let fileOperations = FileOperations (logFilePath, formatMilliseconds, utf8)
    // TODO: LateInit
    let mutable send: Action<obj> = Action<obj>(ignore)

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            let items = fileOperations.GetLines getItems.StartRange getItems.EndRange
            send.Invoke {| Method = Method.Items; Items = items; ReqId = getItems.ReqId |}
            return! loop ()
        }
        loop ()  
    )    

    let mutable refreshMode = true

    let rec startRefresh () = 
        async {
            let lines = fileOperations.Refresh ()
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
        | SetRestriction setRestriction ->
            printfn "Restriction %O" setRestriction.Restriction 
            fileOperations.SetRestrict setRestriction.Restriction
            let lines = fileOperations.LineCount
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
            match setRestriction.Restriction with
            | Some _ -> startRefresh ()
            | None -> refreshMode <- false

    static do 
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance |> ignore

    member this.OnReceive payload =
        let msg = Json.deserializeStreamWithOptions<Message> payload
        onReceive msg

    member this.OnReceive payload =
        let msg = Json.deserialize<Message> payload 
        onReceive msg

    member this.OnClose () =
        ()

    member this.Initialize(sendToSet: Action<obj>) =
        send <- sendToSet 
        let lines = fileOperations.LineCount
        send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
        startRefresh ()

    