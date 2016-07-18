local System = System
local String = System.String
local format = String.format

local io = io
local stdin = io.stdin
local stdout = io.stdout
local read = stdin.read
local write = stdout.write
local tostring = tostring
local select = select
local byte = string.byte

local Console = {}

function Console.read()
    local ch = read(stdin, 1)
    return byte(ch)
end

function Console.readLine()
   return read(stdin)
end

function Console.write(v, ...)
    if select("#", ...) ~= 0 then
        v = format(v, ...)
    else
        v = tostring(v)      
    end
    write(stdout, v)     
end

function Console.writeLine(v, ...)
    if select("#", ...) ~= 0 then
        v = format(v, ...)
    else
        v = tostring(v)      
    end
    write(stdout, v, '\n')     
end

System.define("System.Console", Console)