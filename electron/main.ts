import { app, BrowserWindow, BrowserViewConstructorOptions, ipcMain } from 'electron'
import * as path from "path"
import settings from 'electron-settings'

const createWindow = function() {    
    // if (process.env.NODE_ENV == 'DEV')
    //     require('vue-devtools').install()        

    const bounds = settings.get("window-bounds", { 
        width: 800,
        height: 600,
    }) as BrowserViewConstructorOptions
    const b = bounds as any
//    b.icon = __dirname + '/kirk2.png'
    b.show = false 

    // bounds.webPreferences = {
    //     preload: path.join(__dirname, 'preload.js'),
    //     nodeIntegration: true
    // }        
    
    win = new BrowserWindow(bounds)   
    if (settings.get("isMaximized"))
        win.maximize()

    ipcMain.on("openDevTools",  (evt, arg) => win.webContents.openDevTools())
    ipcMain.on("fullscreen",  (evt, arg) => win.setFullScreen(!win.isFullScreen()))
    ipcMain.on("minimize",  (evt, arg) => win.minimize())
    ipcMain.on("maximize",  (evt, arg) => {
        if (win.isMaximized())
            win.restore()
        else
            win.maximize()  
    })

    win.once('ready-to-show', () => win.show()) 

    win.loadURL(process.env.NODE_ENV != 'DEV'
        ? 'vue://' + path.join(__dirname, `/../../renderer/index.html`)
        : 'http://localhost:8080/')

    win.on('close', () => {
        if (!win.isMaximized()) {
            const bounds = win.getBounds()
            settings.set("window-bounds", bounds as any)
            win.webContents.send("closed")
        }
        //child.send("kill")
    })

    win.on('maximize', () => {
        const bounds = win.getBounds()
        settings.set("window-bounds", bounds as any)
        settings.set("isMaximized", true)
    })

    win.on('unmaximize', () => {
        settings.set("isMaximized", false)
    })    

    win.on("closed", () => {win = null})    

    //initializeMenu(win)
}

app.removeAllListeners('ready')
app.on('ready', createWindow)

app.on("activate", () => {
    if (win === null) 
        createWindow()
})

var win: BrowserWindow