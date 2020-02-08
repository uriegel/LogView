module Requests

open System.IO

type private Line = {
    pos: int64
    length: int64
}

let mutable private logFile = None //: Some FileStream
let mutable private lineIndexes: (Line array) = [||]

let loadLogFile path = 
    logFile <- Some (File.OpenRead path)
    let file = 
        match logFile with
        | Some value -> value
        | _ -> failwith "File not opened"
    let buffer = Array.zeroCreate 200000

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
                        | Some pos -> Some({ pos = state + fileOffset; length = (int64 pos - state - 1L) }, int64 (pos + 1))
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

    lineIndexes <- getLines () |> Seq.toArray
