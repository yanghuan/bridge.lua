local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local FormatException = System.FormatException
local IndexOutOfRangeException = System.IndexOutOfRangeException

local tostring = tostring
local unpack = unpack
local string = string
local schar = string.char
local tinsert = table.insert
local tconcat = table.concat

local String = {}

local function check(s, startIndex, count)
    local len = #s
    startIndex = startIndex or 0
    if startIndex < 0 or startIndex > len then
        throw(ArgumentOutOfRangeException("startIndex"))
    end
    count = count or len - startIndex
    if count < 0 or count > len - startIndex then
        throw(ArgumentOutOfRangeException("count"))
    end
    return startIndex, count, len
end

function String.build(...)
    local len = select("#", ...)
    if len == 2 then
        local c, count = ...
        if count <= 0 then
            throw(ArgumentOutOfRangeException("count"))
        end
        return schar(c):rep(count)
    end
    local value, startIndex, length = ...
    startIndex, length = check(value, startIndex, length)
    return schar(unpack(value, startIndex + 1, startIndex + length))
end

local function compare(strA, strB, ignoreCaseOrType, cultureInfo)
    if strA == nil then
        return(strB == nil) and 0 or -1
    end

    if strB == nil then
        return 1
    end

    if ignoreCaseOrType ~= nil then
        if type(ignoreCaseOrType) == "number" then
            -- StringComparison
            if ignoreCaseOrType % 2 ~= 0 then
                strA = strA:lower()
                strB = strB:lower()
            end
        else
            -- ignoreCase
            if ignoreCaseOrType then
                strA = strA:lower()
                strB = strB:lower()
            end

            if cultureInfo then
                -- CultureInfo
                throw(System.NotSupportedException("cultureInfo is not support"))
            end
        end
    end
    if strA > strB then return 1 end
    if strA < strB then return -1 end
    return 0
end

String.compare = compare

function String.compareTo(this, v)
    return compare(this, v)
end

string.compareTo = String.compareTo

function String.compareToObj(this, v)
    if v == nil then return 1 end
    if type(v) ~= "string" then
        throw(ArgumentException("Arg_MustBeString"))
    end
    return compare(this, v)
end

string.compareToObj = String.compareToObj

function String.equals(this, v, comparisonType)
    return compare(this, v, comparisonType) == 0
end

string.equals = String.equals

function String.equalsObj(this, v)
    if type(v) == "string" then
        return this == v
    end
    return false
end

string.equalsObj = String.equalsObj

function String.getHashCode(this)
    return this
end

string.getHashCode = String.getHashCode

function String.get(this, index)
    if index < 0 or index >= #this then
        throw(IndexOutOfRangeException())
    end
    return this:byte(index + 1)
end

string.get = String.get

function String.concat(...)
    local t = {}
    local len = select("#", ...)
    if len == 1 then
        local v = ...
        if System.isEnumerableLike(v) then
            for _, v in System.each(array) do
                tinsert(t, tostring(v))
            end
        else 
            return tostring(v)
        end
    else
        for i = 1, len do
            local v = select(i, ...)
        tinsert(t, tostring(v))
        end
    end
    return tconcat(t)
end

function String.join(separator, value, startIndex, count)
    local t = {}
    local has
    if startIndex then  
        check(value, startIndex, count)
        for i = startIndex + 1, startIndex + count do
            local v = value:get(i)
            if v ~= nil then
                if has then
                    tinsert(t, separator)
                else 
                    has = true
                end
                tinsert(t, v)
            end
        end
    else
        for _, v in System.each(value) do
            if v ~= nil then
                if has then
                    tinsert(t, separator)
                else 
                    has = true
                end
                tinsert(t, v)
            end      
        end
    end
    return tconcat(t)
end

local function checkIndexOf(str, value, startIndex, count, comparisonType)
    if value == nil then
        throw(ArgumentNullException("value"))
    end
    startIndex, count = check(str, startIndex, count)
    str = str:sub(startIndex + 1, startIndex + count)
    if comparisonType and comparisonType % 2 ~= 0 then
        str = str:lower()
        value = value:lower()
    end
    return str, value, startIndex
end

function String.lastIndexOf(str, value, startIndex, count, comparisonType)
    if type(value) == "number" then
        value = schar(value)
    end
    str, value, startIndex = checkIndexOf(str, value, startIndex, count, comparisonType)
    local index = str:match(".*()" .. value)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

string.get = String.lastIndexOf

local function indexOfAny(str, chars, startIndex, count)
    if chars == nil then
        throw(ArgumentNullException("chars"))
    end
    startIndex, count = check(str, startIndex, count)
    str = str:sub(startIndex + 1, startIndex + count)
    return str, "[" .. schar(unpack(chars)) .. "]", startIndex
end

function String.lastIndexOfAny(str, chars, startIndex, count)
    str, chars, startIndex = indexOfAny(str, chars, startIndex, count)
    local index = str:match("^.*()" .. chars)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

string.get = String.lastIndexOfAny

function String.isNullOrWhiteSpace(value)
    return value == nil or value:find("^%s*$") ~= nil
end

function String.isNullOrEmpty(value)
    return value == nil or #value == 0
end

function String.format(format, ...)
    local len = select("#", ...)
    if len == 1 then
        local v = ...
        if System.isArrayLike(v) then
            return format:gsub("{(%d)}", function(n) 
                local v = v:get(n + 0)   -- make n to number
                if v == nil then
                    throw(FormatException())
                end
                return tostring(v) 
            end)
        end 
    end
    local arg = { ... }
    return format:gsub("{(%d)}", function(n)
        local v = arg[n + 1]
        if v == nil then
            throw(FormatException())
        end
        return tostring(v) 
    end)
end

function String.startsWith(this, prefix)
    return this:sub(1, #prefix) == prefix
end

string.startsWith = String.startsWith

function String.endsWith(this, suffix)
    return suffix == "" or this:sub(-#suffix) == suffix
end

string.endsWith = String.endsWith

function String.contains(this, value)
    if value == nil then
        throw(ArgumentNullException("value"))
    end
    return this:find(value) ~= nil
end

string.contains = String.contains

function String.indexOfAny(str, chars, startIndex, count)
    str, chars, startIndex = indexOfAny(str, chars, startIndex, count)
    local index = str:find(chars)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

string.indexOfAny = String.indexOfAny

function String.indexOf(str, value, startIndex, count, comparisonType)
    if type(value) == "number" then
        value = schar(value)
    end
    str, value, startIndex = checkIndexOf(str, value, startIndex, count, comparisonType)
    local index = str:find(value)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

string.get = String.indexOf

function String.toCharArray(str, startIndex, count)
    startIndex, count = check(str, startIndex, count)
    local t = { }
    for i = startIndex + 1, startIndex + count do
        tinsert(t, str:byte(i))
    end
    return System.arrayFromTable(t, System.Char)
end

local function escape(s)
    return s:gsub("([%%%^%.])", "%%%1")
end

function String.replace(this, a, b)
    if type(a) == "number" then
        a = schar(a)
        b = schar(b)
    end
    a = escape(a)
    return this:gsub(a, b)
end

string.replace = String.replace

function String.insert(this, startIndex, value) 
    if value == nil then
        error(System.new(System.ArgumentNullException, "value"))
    end
    startIndex = check(this, startIndex)
    return this:sub(1, startIndex) .. value .. this:sub(startIndex + 1)
end

string.insert = String.insert

function String.remove(this, startIndex, count) 
    startIndex, count = stringCheck(this, startIndex, count)
    return this:sub(1, startIndex) .. this:sub(startIndex + 1 + count)
end

string.remove = String.remove

function String.substring(this, startIndex, count)
    startIndex, count = check(str, startIndex, count)
    return this:sub(startIndex + 1, startIndex + count)
end

string.substring = String.substring

local function findAny(s, strings, startIndex)
    local findBegin, findEnd, findStr
    for _, str in ipairs(strings) do
        local pattern = escape(str)
        local posBegin, posEnd = string.find(s, pattern, startIndex)
        if posBegin then
            if not findBegin or posBegin < findBegin then
                findBegin, findEnd, findStr = posBegin, posEnd, str
            end
        end
    end
    return findBegin, findEnd, findStr
end

String.findAny = findAny

function String.split(this, strings, count, options) 
    local t = {}
    local find = string.find
    if type(strings) == "table" then
        if #strings == 0 then
            return t
        end  

        if type(strings[1]) == "string" then
            find = findAny
        else
            strings = schar(unpack(strings))
            strings = escape(strings)
            strings = "[" .. strings .. "]"
        end
    elseif type(strings) == "string" then       
        strings = escape(strings)         
    else
        string = schar(strings)
        strings = escape(strings)
    end

    local startIndex = 1
    while true do
        local posBegin, posEnd = find(this, strings, startIndex)
        posBegin = posBegin or 0
        local subStr = this:sub(startIndex, posBegin -1)
        if options ~= 1 or #subStr > 0 then
            tinsert(t, subStr)
            if count then
                count = count -1
                if count == 0 then
                    break
                end
            end  
        end
        if posBegin == 0 then
            break
        end 
        startIndex = posEnd + 1
    end   
    return System.arrayFromTable(t, String) 
end

string.split = String.split

function String.trimEnd(this, chars)
    if chars then
        chars = schar(unpack(chars))
        chars = escape(chars)
        chars = "(.-)[" .. chars .. "]*$"
    else 
        chars = "(.-)%s*$"
    end
    return (this:gsub(chars, "%1"))
end

string.trimEnd = String.trimEnd

function String.trimStart(this, chars) 
    if chars then
        chars = schar(unpack(chars))
        chars = escape(chars)
        chars = "^[" .. chars .. "]*(.-)"
    else 
        chars = "^%s*(.-)"
    end
    return (this:gsub(chars, "%1"))
end

string.trimStart = String.trimStart

function String.trim(this, chars) 
    if chars then
        chars = schar(unpack(chars))
        chars = escape(chars)
        chars = "^[" .. chars .. "]*(.-)[" .. chars .. "]*$"
    else 
        chars = "^%s*(.-)%s*$"
    end
    return (this:gsub(chars, "%1"))
end

string.trim = String.trim

System.define("System.String", String)


