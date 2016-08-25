local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException
local ArgumentNullException = System.ArgumentNullException
local FormatException = System.FormatException
local OverflowException = System.OverflowException

local Int = {}

local function compare(this, v)
    if this < v then return -1 end
    if this > v then return 1 end
    return 0
end

Int.compareTo = compare

function Int.compareToObj(this, v)
   if v == null then return 1 end
   if type(v) ~= "number" then
       throw(ArgumentException("Arg_MustBeInt"))
   end
   return compare(this, v)
end

function Int.equals(this, v)
    return this == v
end

function Int.equalsObj(this, v)
    if type(v) ~= "number" then
        return false
    end
    return this == v
end

function Int.getHashCode(this)
    return this
end

local function parse(s, min, max, safe)
    if s == nil then
        if safe then return
        else
            throw(ArgumentNullException())
        end
    end
    local v = tonumber(s)
    if v == nil or v ~= math.floor(v) then
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

function Int.parse(s, min, max)
    return parse(s, min, max)
end

function Int.tryParse(s, _, min, max)
    local v = parse(s, min, max, true)
    if v then
        return true, v
    end
    return false, 0
end

function Int.__default__()
    return 0
end 

System.defStc("System.Int", Int)
Int.__inherits__ = { System.IComparable, System.IFormattable, System.IComparable_1(Int), System.IEquatable_1(Int) }






