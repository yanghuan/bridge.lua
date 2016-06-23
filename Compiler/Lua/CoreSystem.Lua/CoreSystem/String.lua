local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local FormatException = System.FormatException

local String = {}

function System.checkThis(this)
    if this == nil then
        throw(System.NullReferenceException())
    end
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
                strA = string.lower(strA)
                strB = string.lower(strB)
            end
        else
            -- ignoreCase
            if ignoreCaseOrType then
                strA = string.lower(strA)
                strB = string.lower(strB)
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
    checkThis(this)
    return compare(this, v)
end

function String.compareToObj(this, v)
    checkThis(this)
    if v == nil then return 1 end
    if type(v) ~= "string" then
        throw(ArgumentException("Arg_MustBeString"))
    end
    return compare(this, v)
end

function String.Equals(this, v, comparisonType)
    checkThis(this)
    return compare(this, v, comparisonType) == 0
end

function String.EqualsObj(this, v)
    checkThis(this) 
    if type(v) == "string" then
        return this == v
    end
    return false
end

function String.getHashCode(this)
    return this
end

function String.charCodeAt(s, index)
    if index < 0 or index >= #s then
        throw(System.IndexOutOfRangeException())
    end
    return string.byte(s, index + 1)
end

local function check(s, startIndex, count)
    local len = #s
    startIndex = startIndex or 0
    if startIndex < 0 or startIndex > len then
        throw(System.ArgumentOutOfRangeException("startIndex", "startIndex cannot be less than zero and must refer to a location within the string"))
    end

    count = count or len - startIndex
    if count < 0 or count > len - startIndex then
        throw(System.ArgumentOutOfRangeException("count", "must be non-negative and must refer to a location within the string"))
    end
    return startIndex, count, len
end

local function checkIndexOf(str, value, startIndex, count, comparisonType)
    if value == nil then
        throw(System.ArgumentNullException("value"))
    end

    startIndex, count = check(str, startIndex, count)
    str = string.sub(str, startIndex + 1, startIndex + count)
    if comparisonType and comparisonType % 2 ~= 0 then
        str = string.lower(str)
        value = string.lower(value)
    end
    return str, value, startIndex
end

function String.lastIndexOf(str, value, startIndex, count, comparisonType)
    str, value, startIndex = checkIndexOf(str, value, startIndex, count, comparisonType)
    local index = string.match(str, ".*()" .. value)
    if index then
        return index - 1 + startIndex
    end
    return -1;
end

local function indexOfAny(str, chars, startIndex, count)
    if chars == nil then
        throw(System.ArgumentNullException("chars"))
    end

    startIndex, count = check(str, startIndex, count)
    str = string.sub(str, startIndex + 1, startIndex + count)
    return str, "[" .. string.char(unpack(chars)) .. "]", startIndex
end

function String.lastIndexOfAny(str, chars, startIndex, count)
    str, chars, startIndex = indexOfAny(str, chars, startIndex, count)
    local index = string.match(str, "^.*()" .. chars)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

function String.isNullOrWhiteSpace(value)
    return value == nil or string.find(value, "^%s*$") ~= nil
end

function String.isNullOrEmpty(value)
    return value == nil or #value == 0
end

function String.fromCharCount(c, count)
    if count >= 0 then
        return string.rep(string.char(c), count)
    else
        throw(System.ArgumentOutOfRangeException("count", "cannot be less than zero"))
    end
end

function String.format(format, ...)
    local arg = { ... }
    return string.gsub(format, "{(%d)}", function(n)
        local v = arg[n + 1]
        if v == nil then
            throw(FormatException())
        end
        return tostring(v) 
    end)
end

function String.startsWith(str, prefix)
    return string.find(str, "^" .. prefix) ~= nil
end

function String.endsWith(str, suffix)
    return string.find(str, prefix .. "$") ~= nil
end

function String.contains(str, value)
    if value == nil then
        throw(System.ArgumentNullException("value"))
    end
    return string.find(str, value) ~= nil
end

function String.indexOfAny(str, chars, startIndex, count)
    str, chars, startIndex = indexOfAny(str, chars, startIndex, count)
    local index = string.find(str, chars)
    if index then
        return index - 1 + startIndex
    end
    return -1
end

function String.indexOf(str, value, startIndex, count, comparisonType)
    str, value, startIndex = checkIndexOf(str, value, startIndex, count, comparisonType)
    local index = string.find(str, value)
    if index then
        return index - 1 + startIndex
    end
    return -1;
end

function String.toCharArray(str, startIndex, count)
    startIndex, count = check(str, startIndex, count)

    local arr = { }
    for i = startIndex + 1, startIndex + count do
        table.insert(arr, string.byte(str, i))
    end
    return arr
end

local function escape(str)
    return string.gsub(str, "([%%%^%.])", "%%%1")
end

function String.replaceAll(str, a, b)
    a = escape(a)
    return string.gsub(str, a, b)
end

function String.insert(s, startIndex, value) 
    if value == nil then
        error(System.new(System.ArgumentNullException, "value"))
    end

    startIndex = check(s, startIndex)
    return string.sub(s, 1, startIndex) .. value .. string.sub(s, startIndex + 1)
end

function String.remove(s, startIndex, count) 
    startIndex, count = stringCheck(s, startIndex, count)
    return string.sub(s, 1, startIndex) .. string.sub(s, startIndex + 1 + count)
end

function String.substring(str, startIndex, count)
    startIndex, count = check(str, startIndex, count)
    return string.sub(str, startIndex + 1, startIndex + count)
end

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

function String.split(s, strings, count, options) 
    local arr = {}
    local find = string.find
    if type(strings) == "table" then
        if #strings == 0 then
            return arr
        end  

        if type(strings[1]) == "string" then
            find = findAny
        else
            strings = string.char(unpack(strings))
            strings = escape(strings)
            strings = "[" .. strings .. "]"
        end
    elseif type(strings) == "string" then       
        strings = escape(strings)         
    else
        string = string.char(strings)
        strings = escape(strings)
    end

    local startIndex = 1
    while true do
        local posBegin, posEnd = find(s, strings, startIndex)
        posBegin = posBegin or 0
        local subStr = string.sub(s, startIndex, posBegin -1)
        if options ~= 1 or #subStr > 0 then
            table.insert(arr, subStr)
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
    return System.ArrayFromTable(arr, String) 
end

function String.trimEnd(s, chars)
    if chars then
        chars = string.char(unpack(chars))
        chars = escape(chars)
        chars = "(.-)[" .. chars .. "]*$"
    else 
        chars = "(.-)%s*$"
    end
    return (string.gsub(s, chars, "%1"))
end

function String.trimStart(s, chars) 
    if chars then
        chars = string.char(unpack(chars))
        chars = escape(chars)
        chars = "^[" .. chars .. "]*(.-)"
    else 
        chars = "^%s*(.-)"
    end
    return (string.gsub(s, chars, "%1"))
end

function String.trim(s, chars) 
    if chars then
        chars = string.char(unpack(chars))
        chars = escape(chars)
        chars = "^[" .. chars .. "]*(.-)[" .. chars .. "]*$"
    else 
        chars = "^%s*(.-)%s*$"
    end
    return (string.gsub(s, chars, "%1"))
end

System.define("System.String", String)


