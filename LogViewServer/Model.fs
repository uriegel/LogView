namespace Model
open System

type Method = 
| ChangeTheme = 1
| ItemsSource = 2  
| Items = 3

type MsgType = 
| Trace = 1 
| Info = 2
| Warning = 3
| Error = 4
| Fatal = 5
| NewLine = 6

type LineItem = {
    Index: int
    Text: string
    MsgType: MsgType
}