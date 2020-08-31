namespace ULogViewServer
open System.IO
open System.Text
open FSharpTools
open FSharpTools.String
open System.Globalization

type Line = {
    Text: string
    FileIndex: int
    Index: int
}

type FileOperations(path: string, formatMilliseconds: bool, utf8: bool) = 
    let encoding = if utf8 then Encoding.UTF8 else Encoding.GetEncoding (CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
    let posType = if formatMilliseconds then 24 else 20
    let mutable restriction: string option = None
    let mutable minimalType = MinimalType.Trace

    let stream = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) 
    let reader = new StreamReader (stream)

    let readLines () =
        let mutable run = true
        seq {
            while run do
                let textline = reader.ReadLine ()
                if not (isNull textline) then
                    yield textline
                else 
                    run <- false
        }
        
    let mutable fileLines = 
        readLines () 
        |> Seq.mapi (fun i n -> { Text = n; Index = i; FileIndex = i})
        |> Seq.toArray

    let mutable lines = fileLines

    let getLines (lineRange: Line array) =
        let timeLength = if formatMilliseconds then 12 else 8
        let textPos = if formatMilliseconds then 29 else 25
        lineRange        
        |> Seq.map (fun line -> 
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
                match line.Text |> substring2 posType 5 with
                | "TRACE" -> MsgType.Trace
                | "INFO " -> MsgType.Info
                | "WARNI" -> MsgType.Warning
                | "ERROR" -> MsgType.Error
                | "FATAL" -> MsgType.Fatal
                | _ -> MsgType.NewLine
            {
                Index = line.Index
                LineIndex = line.FileIndex
                ItemParts = getItemParts line.Text msgType
                MsgType = msgType
            })

    member this.LineCount = lines.Length 

    member this.Restriction 
        with get () = restriction
        and set value = restriction <- value

    member this.SetRestrict restriction indexToSelect = 
        this.Restriction <- restriction
        1
        // fileSize <- file.Length
        // file.Position <- 0L
        // lineIndexes <-
        //     file
        //     |> createLogIndexes 
        // match indexToSelect, restriction with
        // | None, _ -> 0
        // | Some index, None -> index    
        // | Some index, Some _ -> 
        //     match lineIndexes |> Array.tryFindIndex (fun n -> n.Index = index) with
        //     | Some value -> value
        //     | None -> 
        //         let sel = lineIndexes |> Array.tryFindIndex (fun n -> n.Index > index)
        //         match sel with
        //         | Some 0 -> 0
        //         | Some sel -> 
        //             let first = lineIndexes.[sel-1]
        //             let sec = lineIndexes.[sel]
        //             if index - first.Index < sec.Index - index then
        //                 sel - 1
        //             else
        //                 sel
        //         | None -> lineIndexes.Length - 1

    member this.SetMinimalType newMinimalType = 
        minimalType <- newMinimalType
        
        let types = 
            match minimalType with 
            | MinimalType.Info -> [| "INFO "; "WARNI"; "ERRRO"; "FATAL" |]
            | MinimalType.Warning -> [| "WARNI"; "ERRRO"; "FATAL" |]
            | MinimalType.Error -> [| "ERRRO"; "FATAL" |]
            | MinimalType.Fatal -> [| "FATAL" |]
            | _ -> [| |]

        //let compareType line typ = line |> String.substring2 posType 5 = typ
        let compareType line typ = 
            let affe = line |> String.substring2 posType 5
            affe = typ

        let filter line = 
            let res =  
                if String.length line.Text > posType + 5 then
                    types 
                    |> Seq.map (compareType line.Text)
                    |> Seq.tryFind id 
                else
                    None
            match res with
                    | Some true -> true
                    | _ -> false

        lines <- 
            if minimalType = MinimalType.Trace then
                fileLines
            else
                fileLines 
                |> Array.filter filter
                |> Array.mapi (fun i n -> { FileIndex = n.FileIndex; Index = i; Text = n.Text })

    member this.Refresh () = 
        // let recentFileSize = fileSize
        // fileSize <- file.Length
        // if recentFileSize < fileSize then
        //     file.Position <- recentFileSize
        //     let newLines =
        //         file 
        //         |> createLogIndexes
        //         |> Seq.toArray
        //     if newLines.Length > 0 then
        //         lineIndexes <- 
        //             [| lineIndexes; newLines |] 
        //             |> Seq.concat 
        //             |> Seq.toArray
        //         lineIndexes.Length
        //     else
        //         0
        // else
            0

    member this.GetLines startIndex endIndex = 
        let startIndex = max 0 startIndex
        let endIndex = max 0 endIndex
        let endIndex = min (lines.Length - 1) endIndex
        let range = lines.[startIndex..endIndex]
        getLines range
        |> Seq.toArray
        

