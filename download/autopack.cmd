set version=1.0.1
set dir=bridge.lua
set file=%dir%.%version%.zip
if exist "%file%" del "%file%"
md %dir%
xcopy "..\Bridge\bin\Debug\Bridge.dll"  %dir%   /y
xcopy "..\Compiler\Lua\bin\Debug"  %dir%  /s /e /y 
set _7z="D:\Program Files\7-Zip\7z"
%_7z% a %file% %dir%
rd /s /q %dir%