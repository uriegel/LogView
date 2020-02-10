module Requests

open System.IO
open System.Text

type LineItem = {
    Index: int
    Text: string
}

let mutable private lineIndexes: int64 array = [||]
let mutable private path = ""
let mutable fileSize = 0L

let getLineCount () = lineIndexes.Length 
let private accessfile adjustLength = 
    let file = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    if adjustLength then
        fileSize <-file.Length
    file

// let private createLogIndexes1 (file: FileStream) =
//     //let buffer = Array.zeroCreate 200000
//     let buffer = Array.zeroCreate 1000

//     let rec findChr index maxLength = 
//         match index < maxLength with
//         | true when buffer.[index] = searchChr -> Some index
//         | true -> findChr buffer (index + 1) maxLength '\n'
//         | false -> None 

//     let rec getNextIndex (index, bufferLength, fileOffset) = 
//         match 
//             if bufferLength = 0 then
//                 let fileOffset = file.Position
//                 if fileOffset < file.Length then
//                     Some (0, file.Read (buffer, 0, buffer.Length))
//                 else
//                     None
//             else
//                 Some (index, bufferLength) with

//         | Some (index, bufferLength) ->
//             match findChr index bufferLength with
//             | Some pos ->  Some (pos + fileOffset, (pos + 1, bufferLength, fileOffset))
//         | None -> None

let private createLogIndexes (file: FileStream) =
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
    path <- logFilePath
    lineIndexes <-
        accessfile true 
        |> createLogIndexes 
        |> Seq.toArray

let refresh () = 
    let recentFileSize = fileSize
    let file = accessfile true 
    file.Position <- recentFileSize
    let newLines =
        file 
        |> createLogIndexes
        |> Seq.toArray
    lineIndexes <- 
        [| lineIndexes; newLines |] 
        |> Seq.concat 
        |> Seq.toArray
    lineIndexes.Length

let getLines startIndex endIndex = 
    let file = accessfile false
    let buffer = Array.zeroCreate 200000    
    let getString startPos length =
        file.Position <- startPos
        file.Read (buffer, 0, length) |> ignore
        Encoding.UTF8.GetString (buffer, 0, length)

    let getNextIndex i =
        if i < lineIndexes.Length - 1 then 
            lineIndexes.[i+1] 
        else 
            fileSize

    let endIndex = if endIndex < lineIndexes.Length then endIndex else lineIndexes.Length - 1
    seq { for i in startIndex .. endIndex -> (lineIndexes.[i], getNextIndex i, i) }
    |> Seq.map (fun (n, m, i) -> {
            Index = i
            Text = getString n (int (m - n))
        })
    
let scanFile () =



    let affe = lineIndexes.[960000]
    let schwein = lineIndexes.[960000 - 1]
    let file = accessfile false
    let buffer = Array.zeroCreate 200000    
    let getString startPos length =
        if length < 0 then
            printf "Blöd"    
        file.Position <- startPos
        file.Read (buffer, 0, length) |> ignore

    getString (9600000L - 20L) 60
    printfn "Sau: %s" (System.Text.Encoding.UTF8.GetString (buffer, 0, 60))

    seq { for i in 0..lineIndexes.Length - 2 -> (lineIndexes.[i], lineIndexes.[i+1], i) }
    |> Seq.map (fun (n, m, i) -> 
            getString n (int (m - n))
            i
        )
    |> Seq.toArray