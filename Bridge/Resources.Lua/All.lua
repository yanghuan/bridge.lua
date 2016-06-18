local originalRequire = require
local baseDir = __CoreSystemDir__ or ""  

local function require(class)
    return originalRequire(baseDir .. "CoreSystem." .. class)
end

require("Core");
require("Interfaces")
require("Exception")
require("Double")
require("Int")
require("String");
require("Char")
require("Boolean")
require("Delegate")
require("Enum")
require("TimeSpan")
require("DateTime")
require("Object")
require("Type")
require("Collections.EqualityComparer")
require("Collection")
require("Collections.Interfaces")
require("Collections.List")
require("Collections.Dictionary")
require("Collections.Queue")
require("Collections.Stack")
require("Collections.HashSet")
require("Collections.LinkedList")
require("Collections.Linq")
require("Array")
require("Math")
require("Text.StringBuilder")
