namespace ULogViewServer
open System.IO
open System.Text
open FSharpTools
open FSharpTools.String
open System.Globalization

type Line = {
    Pos: int64
    Length: int
}

type FileOperations(path: string, formatMilliseconds: bool, utf8: bool) = 
    let mutable restriction: string option = None
    let buffer = Array.zeroCreate 80000    
    let encoding = if utf8 then Encoding.UTF8 else Encoding.GetEncoding (CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
    let file = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    let mutable fileSize = file.Length

    let getString (file: Stream) line =
        file.Position <- line.Pos
        file.Read (buffer, 0, line.Length) |> ignore
        encoding.GetString (buffer, 0, line.Length)

    let getRawLine (file: Stream) line =
        file.Position <- line.Pos
        file.Read (buffer, 0, line.Length) |> ignore
        buffer, line.Length

    let createLogIndexes (file: FileStream) =
        let buffer = Array.zeroCreate 20000

        let getLines () =
            let startOffset = file.Position
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
                            | Some pos -> Some( fileOffset + int64 (pos + 1), int64 (pos + 1))
                            | None -> None
                        )
                    )
                else
                    None

            let indexes = 
                0 |> Seq.unfold (fun _ ->
                    match getLinesBuffer () with
                    | Some lines -> Some (lines, 0)
                    | None -> None) 
                |> Seq.concat
                |> Seq.toArray
            let indexes = Array.concat [ [|startOffset|]; indexes ]

            seq { for i in 1 .. indexes.Length - 1 -> { Pos = indexes.[i-1]; Length = int (indexes.[i] - indexes.[i-1]) }}                
            |> Seq.toArray

        let lineIndexes = getLines ()

        match restriction with
        | Some restriction -> 
            let searchStrings = 
                restriction
                |> String.splitChar '|'
                |> Array.map encoding.GetBytes
            
            let getLine = getRawLine file

            let findSearchStr (buffer: byte array) (searchBuffer: byte array) length =
                let rec compare index pos =
                    if pos = searchBuffer.Length then
                        true
                    else
                        if buffer.[index + pos] = searchBuffer.[pos] then 
                            compare index (pos + 1)
                        else
                            false

                let rec compareFirst pos =
                    if pos >= length - searchBuffer.Length then
                        false
                    else
                        if buffer.[pos] = searchBuffer.[0] then 
                            //Some pos
                            if compare pos 0 then
                                true
                            else 
                                compareFirst (pos + 1)
                        else
                            compareFirst (pos + 1)

                compareFirst 0 

            let filter line = 
                let rawLine, length = getLine line 
                searchStrings |> Array.filter (fun n -> findSearchStr rawLine n length) |> Array.length > 0

            let lineIndexes = 
                lineIndexes 
                |> Array.filter filter

            lineIndexes
        | None -> lineIndexes

    let mutable lineIndexes =
        file
        |> createLogIndexes 

    let getLines (file: Stream) startIndex endIndex =
        let posType = if formatMilliseconds then 24 else 20
        let timeLength = if formatMilliseconds then 12 else 8
        let textPos = if formatMilliseconds then 29 else 25

        let getString = getString file

        let endIndex = if endIndex < lineIndexes.Length then endIndex else lineIndexes.Length - 1
        seq { for i in startIndex .. endIndex -> (lineIndexes.[i], i) }
        |> Seq.map (fun (n, i) -> 
            let text = getString n

            let getItemParts text msgType =
                if msgType <> MsgType.NewLine then 
                    let date = text |> String.substring2 0 10
                    let time = text |> String.substring2 11 timeLength
                    let evtPos = text |> String.indexOf " - "
                    let cat, evt = 
                        match evtPos with
                        | Some evtPos -> 
                            text |> String.substring2 textPos (evtPos - textPos), text |> String.substring (evtPos + 3)
                        | None -> "", text |> String.substring (textPos + 1) 
                    [| date + " " + time; cat; evt |]
                else
                    [| ""; ""; text |]

            let msgType = 
                match text |> substring2 posType 5 with
                | "TRACE" -> MsgType.Trace
                | "INFO " -> MsgType.Info
                | "WARNI" -> MsgType.Warning
                | "ERROR" -> MsgType.Error
                | "FATAL" -> MsgType.Fatal
                | _ -> MsgType.NewLine
            {
                Index = i
                ItemParts = getItemParts text msgType
                MsgType = msgType
            })

    member this.LineCount = lineIndexes.Length 

    member this.Restriction 
        with get () = restriction
        and set value = restriction <- value

    member this.SetRestrict restriction = 
        this.Restriction <- restriction
        fileSize <- file.Length
        file.Position <- 0L
        lineIndexes <-
            file
            |> createLogIndexes 
        ()

    member this.Refresh () = 
        let recentFileSize = fileSize
        fileSize <- file.Length
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

    member this.GetLines startIndex endIndex = 
        getLines file startIndex endIndex
        |> Seq.toArray
        

