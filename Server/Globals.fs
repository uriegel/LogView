module Globals

let debugUrl = "http://localhost:8080"

let port = 9866
#if DEBUG
let serviceUrlBase = sprintf "http://localhost:%d" port
#else
let serviceUrlBase = ""
#endif

let theme = "yaru"
