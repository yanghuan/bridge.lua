local System = System

local io = io
local stdin = io.stdin
local stdout = io.stdout
local stderr = io.stderr
local tostring = tostring

local Console = {}

function Console.read()
    local ch = stdin:read(1)
    return string.byte(ch)
end

function Console.readLine()
   return stdin:read()
end

function Console.write(v)
   stdout:write(tostring(v))     
end

function Console.writeLine(v)
   stdout:write(tostring(v), '\n')     
end

System.define("System.Console", Console)