local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException
local OverflowException = System.OverflowException

local Double = {}
local nan = 0 / 0
local posInf = 1 / 0
local negInf = - 1 / 0
local nanHashCode = {}

--http://lua-users.org/wiki/InfAndNanComparisons
local function isNaN(v)
    return v ~= v
end

Double.isNaN = isNaN

local function compare(this, v)
    if this < v then return -1 end
    if this > v then return 1 end
    if this == value then return 0 end
    
    if isNaN(this) then
        return isNaN(v) and 0 or -1
    else 
       return 1
    end
end

Double.compareTo = compare

function Double.compareToObj(this, v)
   if v == null then return 1 end
   if type(v) ~= "number" then
       throw(ArgumentException("Arg_MustBeNumber"))
   end
   return compare(this, v)
end

local function equals(this, v)
    if this == v then return true end
    return isNaN(this) and isNaN(v)
end

Double.equals = equals

function Double.equalsObj(this, v)
    if type(v) ~= "number" then
        return false
    end
    return equals(this, v)
end

function Double.getHashCode(this)
    return isNaN(this) and nanHashCode or this
end

local function parse(s, min, max, safe)
    if s == nil then
        if safe then return
        else
            throw(ArgumentNullException())
        end
    end
    local v = tonumber(s)
    if v == nil then
        if safe then return
        else
            throw(FormatException())
        end
    end
    if v < min or v > max then
        if safe then return
        else
            throw(OverflowException())
        end
    end
    return v
end

function Double.parse(s, min, max)
    return parse(s, min, max)
end

function Double.tryParse(s, _, min, max)
    local v = parse(s, min, max, true)
    if v then
        return true, v
    end
    return false, 0
end

function Double.isNegativeInfinity(v)
    return v == negInf
end

function Double.isPositiveInfinity(v)
    return v == posInf
end

function Double.isInfinity(v)
    return v == posInf or v == negInf    
end 

function Double.__defaultVal__()
   return 0.0
end

System.defStc("System.Double", Double)
Double.__inherits__ = { System.IComparable, System.IFormattable, System.IComparable_1(Double), System.IEquatable_1(Double) }