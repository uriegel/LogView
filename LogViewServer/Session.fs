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
    SelectedIndex: int option
}

type Message =
    | GetItems of GetItems
    | SetRestriction of SetRestriction
    | SetRefreshMode of SetRefreshMode

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

    let refresh () =
        let lines = fileOperations.Refresh ()
        if lines <> 0 then
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = lines - 1 |} 
    
    let mutable refreshMode = true

    let rec startRefresh () = doRefresh ()

    and doRefresh () = 
        async {
            do! Async.Sleep 500
            if refreshMode then
                refresh ()
                startRefresh ()
        } |> Async.Start        

    let onReceive msg =
        match msg with
        | GetItems getItems -> getItemsQueue.Post getItems   
        | SetRestriction setRestriction ->
            let selectedIndex = fileOperations.SetRestrict setRestriction.Restriction setRestriction.SelectedIndex
            let lines = fileOperations.LineCount
            send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = selectedIndex |} 
        | SetRefreshMode setRefreshMode -> 
            refreshMode <- setRefreshMode.Value
            if refreshMode then startRefresh ()

    static do 
        Encoding.RegisterProvider CodePagesEncodingProvider.Instance |> ignore

    member this.OnReceive payload =
        let msg = Json.deserializeStreamWithOptions<Message> payload
        onReceive msg

    member this.OnReceive payload =
        let msg = Json.deserializeWithOptions<Message> payload 
        onReceive msg

    member this.OnClose () =
        ()

    member this.Initialize(sendToSet: Action<obj>) =
        send <- sendToSet 
        let lines = fileOperations.LineCount
        send.Invoke {| Method = Method.ItemsSource; Count = lines; IndexToSelect = 0 |} 
   