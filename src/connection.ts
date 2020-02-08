export function loadLogFile(path: string) {
    return invoke("loadLogFile", { path })
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