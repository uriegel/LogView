namespace ULogViewServer
open System.IO
open System.Text
open FSharpTools
open FSharpTools.String
open System.Globalization
open System

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
    let reader = new StreamReader (stream, encoding)

    let readLines initialIndex =
        let mutable run = true
        seq {
            while run do
                let textline = reader.ReadLine ()
                if not (isNull textline) then
                    yield textline
                else 
                    run <- false
        }
        |> Seq.mapi (fun i n -> { Text = n; Index = initialIndex + i; FileIndex = initialIndex + i})
        |> Seq.toArray
        
    let mutable fileLines = readLines 0 
    let mutable typedLines = fileLines
    let mutable lines = typedLines

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

    let restrict linesToRestrict (restriction: string option) =
        let orRestrictions = 
            match restriction with
            | Some restriction -> Some <| restriction.Split (" OR ", StringSplitOptions.RemoveEmptyEntries) 
            | None -> None

        let filter (orRestrictions: string array) line = 
            let filter (restriction: string) = 
                let andRestrictions = 
                    restriction.Split (" && ", StringSplitOptions.RemoveEmptyEntries) 

                let filter restriction = 
                    match line.Text |> String.indexOfCompare restriction StringComparison.OrdinalIgnoreCase with
                    | None -> true
                    | _ -> false

                andRestrictions
                |> Array.exists filter |> not
            
            orRestrictions
            |> Seq.exists filter

        match orRestrictions with
        | Some restrictions -> 
            linesToRestrict
            |> Array.filter (filter restrictions)
            |> Array.mapi (fun i n -> { FileIndex = n.FileIndex; Index = i; Text = n.Text })
        | None -> linesToRestrict
          
    let restrictMinimalType linesToRestrict = 
        let types = 
            match minimalType with 
            | MinimalType.Info -> [| "INFO "; "WARNI"; "ERROR"; "FATAL" |]
            | MinimalType.Warning -> [| "WARNI"; "ERROR"; "FATAL" |]
            | MinimalType.Error -> [| "ERROR"; "FATAL" |]
            | MinimalType.Fatal -> [| "FATAL" |]
            | _ -> [| |]

        let compareType line typ = line |> String.substring2 posType 5 = typ

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

        if minimalType = MinimalType.Trace then
            linesToRestrict
        else
            linesToRestrict
            |> Array.filter filter
            |> Array.mapi (fun i n -> { FileIndex = i; Index = i; Text = n.Text })

    member this.LineCount = lines.Length 

    member this.Restriction 
        with get () = restriction
        and set value = restriction <- value

    member this.SetRestrict restriction indexToSelect = 
        this.Restriction <- restriction
        lines <- restrict typedLines restriction
        
        match indexToSelect, restriction with
        | None, _ -> 0
        | Some index, None -> index
        | Some index, Some _ -> 
            match lines |> Array.tryFindIndex (fun n -> n.FileIndex = index) with
            | Some value -> value
            | None -> 
                let sel = lines |> Array.tryFindIndex (fun n -> n.FileIndex > index)
                match sel with
                | Some 0 -> 0
                | Some sel -> 
                    let first = lines.[sel-1].FileIndex
                    let sec = lines.[sel].FileIndex
                    if index - first < sec - index then
                        sel - 1
                    else
                        sel
                | None -> lines.Length

    member this.SetMinimalType newMinimalType = 
        minimalType <- newMinimalType
        typedLines <- restrictMinimalType fileLines
        lines <- restrict typedLines restriction

    member this.Refresh () = 
        let newLines = readLines fileLines.Length
        if newLines.Length > 0 then
            fileLines <- Array.concat [| fileLines; newLines |]
            typedLines <- restrictMinimalType fileLines
            lines <- restrict typedLines restriction
            
            lines.Length
        else
            0

    member this.GetLines startIndex endIndex = 
        let startIndex = max 0 startIndex
        let endIndex = max 0 endIndex
        let endIndex = min (lines.Length - 1) endIndex
        let range = lines.[startIndex..endIndex]
        getLines range
        |> Seq.toArray
        
    // TODO: Restriction: Categories
