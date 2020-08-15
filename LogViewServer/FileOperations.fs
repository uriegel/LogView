namespace ULogViewServer
module FileOperations = 
    open System.IO
    open System.Text
    open FSharpTools

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
        if recentFileSize < fileSize then
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

    let substring200 pos length (str: string) =
        match str with
        | null -> ""
        | _ -> 
            let pos = max 0 pos
            let pos = min pos (str.Length - 1)
            let length = max 0 length
            let length = min length (str.Length - pos - 1)
            str.Substring (pos, length)

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
        |> Seq.map (fun (n, m, i) -> 
            let text = getString n (int (m - n))

            let getText () = String.substring 21
            let msgType = 
                    match text |> substring200 20 5 with
                    | "TRACE" -> MsgType.Trace
                    | "INFO " -> MsgType.Info
                    | "WARNI" -> MsgType.Warning
                    | "ERROR" -> MsgType.Error
                    | "FATAL" -> MsgType.Fatal
                    | _ -> MsgType.NewLine
            let text = 
                if msgType <> MsgType.NewLine then 
                    text |> String.substring 26
                else
                    text
            {
                Index = i
                Text = text
                MsgType = msgType
            })