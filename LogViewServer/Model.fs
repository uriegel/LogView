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

type LineItem = {
    Index: int
    ItemParts: string array
    MsgType: MsgType
}