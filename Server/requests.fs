module Requests

open System.IO
open System.Text

type Line = {
    pos: int64
    length: int
}

type LineItem = {
    Index: int
    Text: string
}

let mutable private logFile = None //: Some FileStream
let mutable private lineIndexes: (Line array) = [||]

let getLineCount () = lineIndexes.Length

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
                        | Some pos -> Some({ pos = state + fileOffset; length = int (int64 pos - state - 1L) }, int64 (pos + 1))
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

let getLines startIndex endIndex = 
    let file = 
        match logFile with
        | Some value -> value
        | _ -> failwith "File not opened"

    let buffer = Array.zeroCreate 200000    
    let getString startPos length =
        file.Position <- startPos
        file.Read (buffer, 0, length) |> ignore
        Encoding.UTF8.GetString (buffer, 0, length)

    let endIndex = if endIndex < lineIndexes.Length then endIndex else lineIndexes.Length - 1
    seq { for i in startIndex .. endIndex -> (lineIndexes.[i], i) }
    |> Seq.map (fun (n, i) -> {
            Index = i
            Text = getString n.pos n.length
        })
    
