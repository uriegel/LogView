module LogView
open System.IO
open System
open System.Runtime.InteropServices

let mutable send = fun (payload: obj) -> ()

let changeTheme theme = send {| Method = "changeTheme"; Theme = theme |} 

let initialize (session: Types.Session) = 
    let onReceive payload =
        ()
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

// TODO: use parallel 8 threads

let loadLogFile logFilePath = 
    printfn "Starte"
    path <- logFilePath
    lineIndexes <-
        accessfile true 
        |> createLogIndexes 
        |> Seq.toArray    
    printfn "Fertig %d" lineIndexes.Length

