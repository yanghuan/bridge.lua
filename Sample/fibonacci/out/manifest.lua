return function(dir)
dir = dir and dir .. '.' or ""
local require = require
local load = function(module) return require(dir .. module) end

load("Program")

System.init{
"Test.Program",
}
end