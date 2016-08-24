package.path = package.path .. ";../__bin/coresystem.lua/?.lua"

require("All")()          -- coresystem.lua/All.lua
require("out.manifest")("out")    

Test.Program.main()    -- run main method

