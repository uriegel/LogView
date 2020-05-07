namespace Model
open System

type Method = 
| ChangeTheme = 1
| ItemsSource = 2  
| Items = 3

type LineItem = {
    Index: int
    Text: string
}