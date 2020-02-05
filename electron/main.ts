import { app, BrowserWindow, BrowserViewConstructorOptions, ipcMain, protocol } from 'electron'
import * as path from "path"
import { get as getPlatform} from './platforms/platform'
import settings from 'electron-settings'
import { Themes } from './themes/themes'
import { initializeMenu } from './menu'

protocol.registerSchemesAsPrivileged([{
    scheme: 'vue', privileges: {standard: true, secure: true }
}])

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

    const platform = getPlatform()
    platform.initializeThemes()
    const theme = platform.getCurrentTheme() 
    const isLightMode = theme == Themes.LinuxLight || theme == Themes.WindowsLight
    b.backgroundColor = isLightMode ? "#fff" : "#1e1e1e" 
    bounds.webPreferences = {
        //preload: path.join(__dirname, 'preload.js'),
        nodeIntegration: true
    }        
    
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

    async function insertCss(theme: Themes) {
        let css
        switch (theme) {
            case Themes.LinuxLight:
                css = './themes/ubuntu'
                break
            case Themes.LinuxDark:
                css = './themes/ubuntudark'
                break
            case Themes.WindowsDark:
                css = './themes/dark'
                break
            case Themes.WindowsLight:
                css = './themes/light'
                break
        }
        let cssTheme = await import(css)
        win.webContents.insertCSS(cssTheme.getCss()) 
    }

    protocol.registerBufferProtocol('vue', (request, callback) => {
        let file = decodeURIComponent(request.url.substr(6))
        if (file[1] == '/')
            file = file[0] + ':' + file.substr(1)
        else if (platform.isLinux)
            file = "/" + file
        else if (file.toLowerCase().endsWith(".theme/")) {
            callback({data: Buffer.from(""), mimeType: 'utf8'})
            insertCss(theme)
        }
    }, (error) => {
        if (error) console.error('Failed to register protocol', error)
    })

    win.once('ready-to-show', () => win.show()) 

    platform.setThemeCallback(insertCss)

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

    setTimeout(() => insertCss(Themes.WindowsDark), 1000)

    initializeMenu(win)
}

app.removeAllListeners('ready')
app.on('ready', createWindow)

app.on("activate", () => {
    if (win === null) 
        createWindow()
})

var win: BrowserWindow