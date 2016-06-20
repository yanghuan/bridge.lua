local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException

local Boolean = {}

local function compare(this, v)
    if this == v then
       return 0
    elseif this == false then
        return -1     
    end
    return 1
end

Boolean.compareTo = compare

function Boolean.compareToObj(this, v)
   if v == null then return 1 end
   if type(v) ~= "boolean" then
       throw(ArgumentException("Arg_MustBeBoolean"))
   end
   return compare(this, v)
end

function Boolean.equals(this, v)
    return this == v
end

function Boolean.equalsObj(this, v)
    if type(v) ~= "boolean" then
        return false
    end
    return this == v
end

function Boolean.getHashCode(this)
    return this
end

local function parse(s, safe)
    if s == nil then
        if safe then return
        else
            throw(ArgumentNullException())
        end
    end
    s = s:lower()
    if s == "true" then
        return true
    elseif s == "false" then
        return false
    end
    if not safe then
        throw(FormatException())
    end
end

function Boolean.parse(s)
    return parse(s)
end

function Boolean.tryParse(s)
    local v = parse(s, true)
    if v ~= nil then
        return true, v
    end
    return false, false
end

Boolean.__defaultVal__ = false

System.defStc("System.Boolean", Boolean)
Boolean.__inherits__ = { System.IComparable, System.IComparable_1(Boolean), System.IEquatable_1(Boolean) }


