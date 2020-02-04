"use strict";
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (Object.hasOwnProperty.call(mod, k)) result[k] = mod[k];
    result["default"] = mod;
    return result;
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const electron_1 = require("electron");
const path = __importStar(require("path"));
const electron_settings_1 = __importDefault(require("electron-settings"));
const createWindow = function () {
    // if (process.env.NODE_ENV == 'DEV')
    //     require('vue-devtools').install()        
    const bounds = electron_settings_1.default.get("window-bounds", {
        width: 800,
        height: 600,
    });
    const b = bounds;
    //    b.icon = __dirname + '/kirk2.png'
    b.show = false;
    // bounds.webPreferences = {
    //     preload: path.join(__dirname, 'preload.js'),
    //     nodeIntegration: true
    // }        
    win = new electron_1.BrowserWindow(bounds);
    if (electron_settings_1.default.get("isMaximized"))
        win.maximize();
    electron_1.ipcMain.on("openDevTools", (evt, arg) => win.webContents.openDevTools());
    electron_1.ipcMain.on("fullscreen", (evt, arg) => win.setFullScreen(!win.isFullScreen()));
    electron_1.ipcMain.on("minimize", (evt, arg) => win.minimize());
    electron_1.ipcMain.on("maximize", (evt, arg) => {
        if (win.isMaximized())
            win.restore();
        else
            win.maximize();
    });
    win.once('ready-to-show', () => win.show());
    win.loadURL(process.env.NODE_ENV != 'DEV'
        ? 'vue://' + path.join(__dirname, `/../../renderer/index.html`)
        : 'http://localhost:8080/');
    win.on('close', () => {
        if (!win.isMaximized()) {
            const bounds = win.getBounds();
            electron_settings_1.default.set("window-bounds", bounds);
            win.webContents.send("closed");
        }
        //child.send("kill")
    });
    win.on('maximize', () => {
        const bounds = win.getBounds();
        electron_settings_1.default.set("window-bounds", bounds);
        electron_settings_1.default.set("isMaximized", true);
    });
    win.on('unmaximize', () => {
        electron_settings_1.default.set("isMaximized", false);
    });
    win.on("closed", () => { win = null; });
    //initializeMenu(win)
};
electron_1.app.removeAllListeners('ready');
electron_1.app.on('ready', createWindow);
electron_1.app.on("activate", () => {
    if (win === null)
        createWindow();
});
var win;
//# sourceMappingURL=main.js.map