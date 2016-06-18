local System = System
local throw = System.throw
local Collection = System.Collection
local addCount = Collection.addCount
local clearCount = Collection.clearCount
local ArgumentNullException = System.ArgumentNullException

local StringBuilder = {}

local function build(this, value, startIndex, length)
    value = System.String.substring(value, startIndex, length)
    local len = #value
    if len > 0 then
        this.buffer = { value } 
        addCount(this, len) 
    else
        this.buffer = {} 
    end
end

function StringBuilder.__ctor__(this, ...)
    local len = select("#", ...)
    if len == 0 then
        build(this, "", 0, 0)
    elseif len == 1 or len == 2 then
        local value = ...
        if type(value) == "string" then
            build(this, value, 0, #value)
        else
            build(this, "", 0, 0)
        end
    else 
        local value, startIndex, length = ...
        build(this, value, startIndex, length)
    end
end

StringBuilder.getLength = Collection.getCount

function StringBuilder.append(this, ...)
    local len = select("#", "...")
    if len == 1 then
        local value = ...
        if value ~= nil then
            value = tostring(value)
            table.insert(this.buffer, value)
            addCount(this, #value) 
        end
    else
        local value, startIndex, length = ...
        if value == nil then
            throw(ArgumentNullException("value"))
        end
        value = System.String.substring(value, startIndex, length)
        table.insert(this.buffer, value)
        addCount(this, #value) 
    end
    return this
end

function StringBuilder.appendFormat(this, format, ...)
    local value = System.String.format(format, ...)
    table.insert(this.buffer, this)
    addCount(this, #value) 
    return this
end

function StringBuilder.appendLine(this, value)
    local count = 1;
    if value ~= nil then
        table.insert(this.buffer, value)
        count = count + #value
    end
    table.insert(this.buffer, "\n")
    addCount(this, count) 
    return this
end

function StringBuilder.clear(this)
    this.buffer = {}
    clearCount(this)
    return this
end

function StringBuilder.toString(this)
    return table.concat(this.buffer)
end

StringBuilder.__tostring = StringBuilder.toString
StringBuilder.__len = StringBuilder.getLength

System.define("System.StringBuilder", StringBuilder)
 