local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException

local type = type

local Boolean = {}

local function compare(this, v)
    if this == v then
       return 0
    elseif this == false then
        return -1     
    end
    return 1
end

Boolean.CompareTo = compare

function Boolean.CompareToObj(this, v)
   if v == null then return 1 end
   if type(v) ~= "boolean" then
       throw(ArgumentException("Arg_MustBeBoolean"))
   end
   return compare(this, v)
end

function Boolean.Equals(this, v)
    return this == v
end

function Boolean.EqualsObj(this, v)
    if type(v) ~= "boolean" then
        return false
    end
    return this == v
end

function Boolean.GetHashCode(this)
    return this
end

local function parse(s)
    if s == nil then
        return nil, 1
    end
    s = s:lower()
    if s == "true" then
        return true
    elseif s == "false" then
        return false
    end
    return nil, 2
end

function Boolean.Parse(s)
    local v, err = parse(s)
    if v == nil then
        if err == 1 then
            throw(ArgumentNullException()) 
        else
            throw(FormatException())
        end
    end
    return v
end

function Boolean.TryParse(s)
    local v = parse(s)
    if v ~= nil then
        return true, v
    end
    return false, false
end

function Boolean.__default__()
    return false
end

System.defStc("System.Boolean", Boolean)
Boolean.__inherits__ = { System.IComparable, System.IComparable_1(Boolean), System.IEquatable_1(Boolean) }


