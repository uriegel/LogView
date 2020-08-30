namespace ULogViewServer
open System

type Method = 
| ItemsSource = 1
| Items = 2

type MsgType = 
| Trace = 1 
| Info = 2
| Warning = 3
| Error = 4
| Fatal = 5
| NewLine = 6

type MinimalType = 
| Trace = 0 
| Info = 1
| Warning = 2
| Error = 3
| Fatal = 4

type LineItem = {
    Index: int
    LineIndex: int
    ItemParts: string array
    MsgType: MsgType
}

   