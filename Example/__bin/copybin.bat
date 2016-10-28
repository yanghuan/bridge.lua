del /s /q bridge.lua\*
xcopy "..\..\Compiler\Lua\bin\Debug"  "bridge.lua"  /s /e /y 
del /s /q coresystem.lua\*
xcopy "..\..\Compiler\Lua\CoreSystem.Lua"  "coresystem.lua"  /s /e /y 