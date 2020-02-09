export interface LineItem {
    index: number,
    text: string
}

export async function loadLogFile(path: string) {
    const res = await invokeGetString("loadLogFile", { path })
    const lines = JSON.parse(res) as any
    return Number.parseInt(lines.lineCount)
}

export async function refresh() {
    const res = await invokeGetString("refresh", null)
    const lines = JSON.parse(res) as any
    return Number.parseInt(lines.lineCount)
}

export async function getLines(startRange: number, endRange: number) {
    const res = await invokeGetString("getLines", { startRange, endRange })
    return JSON.parse(res) as LineItem[]
}

export async function scanFile() {
    const res = await invokeGetString("scanFile", null)
    const test = JSON.parse(res) 
    console.log(test)
}

function invokeGetString(method: string, params: any) {
    return new Promise<string>((resolve, _) => {
        const url = new URL(`${urlBase}/request/${method}`)
        if (params)
            url.search = new URLSearchParams(params).toString()

        var xmlhttp = new XMLHttpRequest()
        xmlhttp.onload = _ => resolve(xmlhttp.responseText)
        xmlhttp.open('GET', url.toString(), true)
        xmlhttp.send()
    })
}

function invoke(method: string, param: any) {
    return new Promise((resolve, _) => {
        var xmlhttp = new XMLHttpRequest()
        xmlhttp.onload = _ => {
            var result = JSON.parse(xmlhttp.responseText)
            resolve(result)
        }
        xmlhttp.open('POST', `${urlBase}/request/${method}`, true)
        xmlhttp.setRequestHeader('Content-Type', 'application/json; charset=utf-8')
        xmlhttp.send(JSON.stringify(param))
    })
}

var urlBase = "http://localhost:20000"