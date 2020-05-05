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
        let headerBar = Builder.GetObject (builder, "header")
        GObject.Unref builder
        let webView = WebKit.New ()

        let clearCache () = 
            let context = WebKitContext.GetDefault ()
            WebKitContext.ClearCache context

        let openFile file = 
            HeaderBar.SetSubtitle (headerBar, file)
            LogView.loadLogFile file

        let chooseFile () =
            let dialog = Dialog.NewFileChooser ("Datei öffnen", window, Dialog.FileChooserAction.Open, "_Abbrechen", Dialog.ResponseId.Cancel, "_Öffnen", Dialog.ResponseId.Ok, IntPtr.Zero)
            let res = Dialog.Run dialog
            if res = Dialog.ResponseId.Ok then
                let ptr = Dialog.FileChooserGetFileName dialog
                let file = Marshal.PtrToStringUTF8 ptr
                openFile file
                GObject.Free ptr
            Widget.Destroy dialog

        let onDropFiles (w: nativeint) context x y (data: nativeint) = 
            let files = 
                SelectionData.GetText data
                |> String.splitChar '\n'
            let file = files.[0] |> String.substring 7
            openFile file
        
        Gtk.SignalConnect (window, "drag-data-received", DropFilesFunc onDropFiles)            

        let actions = [
            GtkAction ("destroy", (fun () -> Application.Quit app), "<Ctrl>Q")  
            GtkAction ("openfile", (fun () -> chooseFile ()), "<Ctrl>O")  
#if DEBUG            
            GtkAction ("devtools", (fun () -> WebKit.RunJavascript (webView, "alert('devTools')") |> ignore), "F12")         
#endif            
//             GtkAction ("themes", theme, changeTheme)
        ]
        Application.AddActions (app, actions)
        
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

