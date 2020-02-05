import { Menu, BrowserWindow } from "electron"

export function initializeMenu(win: BrowserWindow) {
    const menu = Menu.buildFromTemplate([
        {
            label: '&Datei',
            submenu: [{
                label: '&Beenden',
                accelerator: 'Alt+F4',
                role: "quit"
            }
        ]},
        {
            label: '&Ansicht',
            submenu: [{
                label: '&Zoomlevel',
                type: "submenu",
                submenu: [{
                    label: '50%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(0.5)
                },
                {
                    label: '75%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(0.75)
                },
                {
                    label: '100%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(1.0)
                },
                {
                    label: '150%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(1.5)
                },
                {
                    label: '200%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(2.0)
                },
                {
                    label: '250%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(2.5)
                },
                {
                    label: '300%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(3.0)
                },
                {
                    label: '350%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(3.5)
                },
                {
                    label: '400%',
                    type: "radio",
                    click: evt => win.webContents.setZoomFactor(4.0)
                }]
            },
            {
                label: '&Vollbild',
                click: () => win.setFullScreen(!win.isFullScreen()),
                accelerator: "F11"
            },
            {
                type: 'separator'
            },            
            {
                label: '&Entwicklerwerkzeuge',
                click: () => win.webContents.openDevTools(),
                accelerator: "F12"
            }
        ]}
    ])
    
    Menu.setApplicationMenu(menu)    
}