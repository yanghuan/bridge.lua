package.path = package.path .. ";D:/Project/Bridge.lua/Compiler/Lua/CoreSystem.Lua/?.lua"

require("All")("");

print("-----------------", "dateTime & timeSpan")

local date = System.DateTime.getNow()
print(date, date:getYear(), date:getMonth(), date:getDay(), date:getMinute(), date:getSecond())

local ts = System.TimeSpan.fromSeconds(20)
print(ts)

date = date + System.TimeSpan.fromDays(2)
print(date)

local baseTime = System.DateTime(1970, 1, 1) 
print(baseTime:addMilliseconds(1458032204643))


print("-----------------", "array")

local arr = System.Array(System.Int)(10)
print(getmetatable(arr).set)
print(#arr)

arr:set(0, nil)
arr:set(1, nil)
print(arr:get(0))

for _, i in System.each(arr) do
    print(i)
end

print("-----------------", "list")

local list = System.List(System.Object)()
list:add(12)
list:add(4)
list:add(3)
list:add(123234)
list:add(123)
list:add(10)

for _, i in System.each(list) do
    print(i)
end

print("concat\n")

for _, i in System.each(System.Linq(list):concat(list)) do
    print(i)
end

print("-----------------", "Linq")
local en = System.Linq(list):skip(1):take(4):where(function(i) return i ~= nil and i >= 4 end)
for _, i in System.each(en) do
    print(i)
end

print("max", System.Linq(list):max())
print("min", System.Linq(list):min())

local list = System.List(System.Object)()
list:add({  k = 1, v = 2 })
list:add({  k = 1, v = 3 })
list:add({  k = 1, v = 4 })
list:add({  k = 2, v = 20 })
list:add({  k = 2, v = 3 })
list:add({  k = 2, v = 4 })

local dict = System.Linq(list):groupBy(function(item)
   return item.k
end, System.Int):toDictionary(function(i) return i.key end, function(i) return System.Linq(i:toArray()):sum(function(i) return i.v end) end, System.Int, System.Int)

for _, v in System.each(dict) do
    print(v.key, v.value)
end

print("-----------------", "Dictionary")


local dict = System.Dictionary(System.Int, System.Int)()
dict:add("a", 1234)
dict:add("b", 1234)
dict:add("c", nil)

for _,  pair in System.each(dict) do
    print(pair.key, pair.value)
end

print("Dictionary & pairs")

for k, v in System.pairs(dict) do
    print(k, v)
end

print("-----------------", "List&String")

local list = System.List(System.String)()
list:add(3)
list:add(4)
list:add(3)
list:add(nil)
list:add(nil)

print(list:contains(nil))


print("-----------------", "HashSet")

local set = System.HashSet(System.Object)()
set:add(1)
set:add(4)

set:symmetricExceptWith(list)

for _, v in System.each(set) do
    print(v)
end

print(#set)
print(set:isSubsetOf(set))


print("-----------------", "LinkList")
local link = System.LinkedList(System.Object)()
local node = link:addFirst(2)
link:addFirst(3)
link:addLast(567)
link:addLast(56)
link:addLast(nil)

link:remove(node)
link:removeFirst()
link:removeLast()

for _, v in System.each(link) do
    print(v)
end

print("LinkList.count", #link)

print("-----------------", "yeild")
function yieldFn() 
    for i = 1, 10 do
       System.yieldReturn(i) 
    end
end

for _, i in System.each(System.yieldEnumerator(yieldFn)) do
    print(i)
end

print("-----------------", "StringBuilder")
local sb = System.StringBuilder()
sb:append("aaaa"):append("kk"):append(true)
print(sb, #sb)


print("-----------------", "Delegate")

local d1 = function() print("d1") end
local d2 = function() print("d2") end
local d3 = function() print("d3") end

System.fn.combine(nil, d1)()
print("--")
System.fn.combine(d1, nil)()
print("--")
System.fn.combine(d1, d2)()
print("--")
System.fn.combine(d1, System.fn.combine(d2, d3))()
print("--")
System.fn.combine(System.fn.combine(d1, d2), System.fn.combine(d2, d3))()
print("--")
System.fn.remove(System.fn.combine(d1, d2), d1)()
print("--")
System.fn.remove(System.fn.combine(d1, d2), d2)()
print("--")
System.fn.remove(System.fn.combine(System.fn.combine(d1, d2), d1), d1)()
print("--")
System.fn.remove(System.fn.combine(System.fn.combine(d1, d2), d3), System.fn.combine(d1, d2))()
print("--")
System.fn.remove(System.fn.combine(System.fn.combine(d1, d2), d3), System.fn.combine(d2, d1))()
print("--")
fn0 = System.fn.combine(System.fn.combine(d1, d2), System.fn.combine(System.fn.combine(d3, d1), d2))
fn1 = System.fn.combine(d1, d2)
System.fn.remove(fn0, fn1)()
print("--")
local i = System.fn.remove(System.fn.combine(d1, d2), System.fn.combine(d1, d2))
print(i == nil)


print("-----------------", "Type")

local ins = 2
ins = System.DateTime.getNow()
ins = System.Dictionary(System.Int, System.String)()
local t = System.getType(ins)
print(t:getName())
print(System.is(2, System.String))
print(System.as("yang", System.String))
print(System.cast("huan", System.String))

print("-----------------", "Console")

--[[
local v = System.Console.readLine()
System.Console.writeLine(v)
System.Console.writeLine("{0}---> {1}", "yes?", false)
--]]

print("------------------------------", "loadBattleLua")
package.path = package.path .. ";D:/testlua/sample/battle/out/?.lua"
ProtoBuf = {}
ProtoBuf.IExtensible = {}
require "manifest" ()



