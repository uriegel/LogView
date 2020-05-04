module Globals

let debugUrl = "http://localhost:8080"

let port = 9865
#if DEBUG
let serviceUrlBase = sprintf "http://localhost:%d" port
#else
let serviceUrlBase = ""
#endif
