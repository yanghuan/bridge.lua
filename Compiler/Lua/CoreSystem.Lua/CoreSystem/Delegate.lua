local System = System
local throw = System.throw
local setmetatable = setmetatable
local getmetatable = getmetatable
local insert = table.insert
local ipairs = ipairs
local assert = assert

local Delegate = {}
Delegate.__index = Delegate

local multicast = setmetatable({}, Delegate)
local memberMethod = setmetatable({}, Delegate)

function multicast.__call(t, ...)
    local result
    for _, f in ipairs(t) do
        result = f(...)
    end
    return result
end

function memberMethod.__call(t, ...)
    return t.method(t.target, ...)
end

local function appendFn(t, f)
    if getmetatable(f) == multicast then
        for _, i in ipairs(f) do
            insert(t, i)
        end
    else
        insert(t, f)
    end
end

local function combine(fn1, fn2)    
    local t = {}
    setmetatable(t, multicast)
    appendFn(t, fn1)
    appendFn(t, fn2)
    return t
end

function Delegate.combine(fn1, fn2)
     if fn1 ~= nil then
        if fn2 ~= nil then 
            return combine(fn1, fn2) 
        end
        return fn1 
    end
    if fn2 ~= nil then return fn2 end
    return nil
end

function Delegate.bind(target, method)
    if target == nil then
        throw(System.ArgumentNullException())
    end
    assert(method)
    local t = {
      target = target,
      method = method,
    }
    setmetatable(t, memberMethod)
    return t
end

local function equalsSingle(fn1, fn2)
    if getmetatable(fn1) == memberMethod then
        if getmetatable(fn2) == memberMethod then
            return fn1.target == fn2.target and fn1.method == fn2.method
        end
        return false 
    end
    if getmetatable(fn2) == memberMethod then return false end
    return fn1 == fn2
end

local function equalsMulticast(fn1, fn2, start, count)
    for i = 1, count do
        if not equalsSingle(fn1[start + i], fn2[i]) then
            return false
        end
    end
    return true
end

local function delete(fn, count, deleteIndex, deleteCount)
    local t = {}
    setmetatable(t, multicast)
    for i = 1, deleteIndex - 1 do
        insert(t, fn[i])
    end
    for i = deleteIndex + deleteCount, count do
        insert(t, fn[i])
    end
    return t
end

local function remove(fn1, fn2) 
    if getmetatable(fn2) ~= multicast then
        if getmetatable(fn1) ~= multicast then
            if equalsSingle(fn1, fn2) then
                return nil
            end
        else
            local count = #fn1
            for i = count, 1, -1 do
                if equalsSingle(fn1[i], fn2) then
                    if count == 2 then
                        return fn1[3 - i]
                    else
                        return delete(fn1, count, i, 1)
                    end
                end
            end
        end
    elseif getmetatable(fn1) == multicast then
        local count1, count2 = #fn1, # fn2
        local diff = count1 - count2
        for i = diff + 1, 1, -1 do
            if equalsMulticast(fn1, fn2, i - 1, count2) then
                if diff == 0 then 
                    return nil
                elseif diff == 1 then 
                    return fn1[i ~= 1 and 1 or count1] 
                else
                    return delete(fn1, count1, i, count2)
                end
            end
        end
    end
    return fn1
end

function Delegate.remove(fn1, fn2)
    if fn1 ~= nil then
        if fn2 ~= nil then
            return remove(fn1, fn2)
        end
        return fn1
    end
    return nil
end

local function equals(fn1, fn2)
    if getmetatable(fn1) == multicast then
        if getmetatable(fn2) == multicast then
            local len1, len2 = #fn1, #fn2
            if len1 ~= len2 then
                return false         
            end
            for i = 1, len1 do
                if not equalsSingle(fn1[i], fn2[2]) then
                    return false
                end
            end
            return true
        end
        return false
    end
    if getmetatable(fn2) == multicast then return false end
    return equalsSingle(fn1, fn2)
end

multicast.__eq = equals
memberMethod.__eq = equals

function Delegate.equalsObj(this, obj)
    if this == nil then
        throw(System.NullReferenceException())
    end
    local typename = type(obj)
    if typename == "function" then
        return equals(this, obj)
    end
    if typename == "table" then
        local metatable = getmetatable(obj)
        if metatable == multicast or metatable == memberMethod then
            return equals(this, obj)
        end
    end
    return false
end

System.fn = Delegate
System.define("System.Delegate", Delegate);
