module FileOperations
open System.IO
open System.Text
open Model

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

let loadLogFile logFilePath = async {
    path <- logFilePath
    lineIndexes <-
        accessfile true 
        |> createLogIndexes 
        |> Seq.toArray    
    return lineIndexes.Length 
} 

let refresh () = 
    let recentFileSize = fileSize
    let file = accessfile true 
    if fileSize < recentFileSize then
        file.Position <- recentFileSize
        let newLines =
            file 
            |> createLogIndexes
            |> Seq.toArray
        if newLines.Length > 0 then
            lineIndexes <- 
                [| lineIndexes; newLines |] 
                |> Seq.concat 
                |> Seq.toArray
            lineIndexes.Length
        else
        0
    else
        0

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