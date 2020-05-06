module LogView
open System.IO
open System
open System.Runtime.InteropServices
open Model

type GetItems = {
    ReqId: int
    StartRange: int
    EndRange: int
}

type Message =
    | GetItems of GetItems

let mutable send = fun (payload: obj) -> ()

let changeTheme theme = send {| Method = "changeTheme"; Theme = theme |} 

let initialize (session: Types.Session) = 

    let getItemsQueue = MailboxProcessor<GetItems>.Start (fun queue ->
        let rec loop () = async {
            let! getItems = queue.Receive ()
            // let items = 
            //     match session.Items with
            //     | NoItems -> [||]
            //     | DriveItems items -> Root.getItems runtime items getItems.StartRange getItems.EndRange
            //     | FileSystemItems items -> FileSystem.getItems items getItems.StartRange getItems.EndRange
            // let payload = { Method = Method.Items; Items = items; ReqId = getItems.ReqId }
            // send (payload :> obj)
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

let mutable private lineIndexes: int64 array = [||]
let mutable private path = ""
let mutable fileSize = 0L

let private accessfile adjustLength = 
    let file = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    if adjustLength then
        fileSize <-file.Length
    file

let private createLogIndexes (file: FileStream) =
    let buffer = Array.zeroCreate 20000

    let getLines () =
        let getLinesBuffer () =
            let fileOffset = file.Position
            if fileOffset < file.Length then
                let read = file.Read (buffer, 0, buffer.Length)

                let rec findChr (buffer: byte array) index maxLength searchChr = 
                    match index < maxLength with
                    | true when buffer.[index] = searchChr -> Some index
                    | true -> findChr buffer (index + 1) maxLength searchChr
                    | false -> None 

                Some (0L  // Initial state
                    |> Seq.unfold (fun state ->
                        match findChr buffer (int state) read (byte '\n') with
                        | Some pos -> Some( state + fileOffset, int64 (pos + 1))
                        | None -> None
                    )
                )
            else
                None

        let ret = 
            0 |> Seq.unfold (fun _ ->
                match getLinesBuffer () with
                | Some lines -> Some (lines, 0)
                | None -> None) |> Seq.concat
        ret

    getLines ()

let loadLogFile logFilePath = 
    async {
        path <- logFilePath
        lineIndexes <-
            accessfile true 
            |> createLogIndexes 
            |> Seq.toArray    
        send {| Method = "itemsSource"; Count = lineIndexes.Length; IndexToSelect = lineIndexes.Length - 1 |} 
    } |> Async.Start
    // TODO: respond default theme
    // TODO: getItems
    // TODO: Process indicator while loading file

