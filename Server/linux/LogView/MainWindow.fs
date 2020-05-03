module Mainwindow
open GtkDotNet
open System
open System.Runtime.InteropServices

type ScriptDialogFunc = delegate of nativeint * nativeint -> bool
type DropFilesFunc = delegate of nativeint * nativeint * int * int * nativeint-> unit
type ConfigureEventFunc = delegate of unit -> bool
type BoolFunc = delegate of unit -> bool

let run () = 
    let app = Application.New "de.hotmail.uriegel.commander"

    let onActivate () =
        let builder = Builder.New ()
        Builder.AddFromFile (builder, "Server/app.glade", IntPtr.Zero) |> ignore
        let window = Builder.GetObject (builder, "window")
        GObject.Unref builder
        let webView = WebKit.New ()

        let clearCache () = 
            let context = WebKitContext.GetDefault ()
            WebKitContext.ClearCache context

//         let actions = [
//             GtkAction ("destroy", (fun () -> Application.Quit app), "<Ctrl>Q")  
//             GtkAction("showhidden", false, showHidden, "<Ctrl>H")                   
//             GtkAction("refresh", refresh, "<Ctrl>R")                   
// #if DEBUG            
//             GtkAction ("test", test, "<Ctrl>T") 
//             GtkAction ("devtools", (fun () -> WebKit.RunJavascript (webView, "alert('devTools')") |> ignore), "F12")         
// #endif            
//             GtkAction ("clearCache", clearCache, "<Ctrl><Shift>Delete") 
//             GtkAction ("themes", theme, changeTheme)
//         ]
//         Application.AddActions (app, actions)
        
        let w = Settings.getDef<int> "Width" 600
        let h = Settings.getDef<int> "Height" 800
        Window.SetDefaultSize (window, w, h)

        let settings = WebKit.GetSettings webView
        GObject.SetBool (settings, "enable-developer-extras", true)
        Container.Add (window, webView)
        Application.AddWindow (app, window)

        let target = TargetEntry.New ("text/plain", TargetEntry.Flags.OtherApp, 0)
        DragDrop.UnSet webView
        DragDrop.SetDestination (window, DragDrop.DefaultDestination.Drop ||| DragDrop.DefaultDestination.Highlight ||| DragDrop.DefaultDestination.Motion,
            target, 1, DragDrop.DragActions.Move)
        TargetEntry.Free target
        
        let onDropFiles (w: nativeint) context x y (data: nativeint) = 
            let files = 
                SelectionData.GetText data
                |> String.splitChar '\n'
            ()
        
        Gtk.SignalConnect (window, "drag-data-received", DropFilesFunc onDropFiles)

        let scriptHook (n: nativeint) dialog =
            let ptr = WebKit.ScriptDialogGetMessage dialog
            if Marshal.PtrToStringUTF8 ptr = "devTools" then
                let inspector = WebKit.GetInspector webView
                WebKit.InspectorShow inspector
            true
        Gtk.SignalConnect (webView, "script-dialog", ScriptDialogFunc scriptHook)
        
        let onWindowStateChanged () =
            let mutable width = 0
            let mutable height = 0
            Window.GetSize (window, &width, &height)
            Settings.setInt "Width" width
            Settings.setInt "Height" height
            false

        Gtk.SignalConnect (window, "configure_event", ConfigureEventFunc onWindowStateChanged)
        Gtk.SignalConnect (webView, "context-menu", BoolFunc (fun () -> true))
        
        Widget.ShowAll window

    #if DEBUG
        let uri = Globals.debugUrl + "/"
    #else
        let uri = sprintf "http://localhost:%d/" Globals.port
    #endif
        WebKit.LoadUri (webView, uri) |> ignore

    Application.Run (app, (fun () -> onActivate ())) |> ignore

