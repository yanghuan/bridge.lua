local System = System
local throw = System.throw
local ArgumentException = System.ArgumentException

local EqualityComparer = {}

local function sampleEquals(t1, t2)
    return t1 == t2
end

local function sampleGetHashCode(t)
    return t
end

function EqualityComparer.__ctor__(this)
    local T = this.__genericT__
    local equals = T.equals or sampleEquals
    local getHashCode = T.getHashCode or sampleGetHashCode

    this.equals = function(x, y)
        if x ~= nil then
            if y ~= nil then return equals(x, y) end
            return false
        end                 
        if y ~= nil then return false end
        return true
    end

    this.getHashCode = function(x)
        if x == nil then return 0 end
        return getHashCode(x)
    end
end

System.define("System.EqualityComparer_1", function(T)
    local cls = {
        __inherits__ = { System.IEqualityComparer_1(T) }, 
        __genericT__ = T,
    }
    local defaultComparer
    function cls.getDefault()
        local comparer = defaultComparer 
        if comparer == nil then
             comparer = System.EqualityComparer_1(T)()
             defaultComparer = comparer
        end
        return comparer
    end
    return cls
end, EqualityComparer)

local Comparer = {}

local compareToObjTable = {
    number = System.Double.compareToObj,
    string = System.String.compareToObj ,
    boolean = System.Boolean.compareToObj,
}

local function getCompareToObj(t)
    local typename = type(t)    
    local fn = compareToObjTable[typename]
    if fn ~= nil then return fn end
    if typename == "table" then 
        return t.compareToObj 
    end
end

local function compare(a, b)
    if a == b then return 0 end
    if a == nil then return -1 end
    if b == nil then return 1 end
    local ia = getCompareToObj(a)
    if ia ~= nil then
        return ia(a, b)
    end
    local ib = getCompareToObj(b)
    if ib ~= nil then
        return -ib(b, a)
    end
    throw(ArgumentException("Argument_ImplementIComparable"))
end

Comparer.compare = compare
System.compare = compare

function Comparer.__ctor__(this)
    if this.__genericT__  ~= nil then
        local compareTo = this.__genericT__.compareTo
        if compareTo ~= nil then
            this.compare = function(x, y)
                if x ~= nil then
                    if y ~= nil then 
                        return compareTo(x, y) 
                    end
                    return 1
                end                 
                if y ~= nil then return -1 end
                return 0
            end
        end
    end
end

local defaultComparerOfComparer

function Comparer.getDefault()
    local comparer = defaultComparerOfComparer
    if comparer == nil then
        comparer = Comparer()
        defaultComparerOfComparer = comparer;
    end
    return comparer
end

System.define("System.Comparer", Comparer)

System.define("System.Comparer_1", function(T)
    local cls = {
        __inherits__ = { System.IComparer_1(T) }, 
        __genericT__ = T,
    }
    local defaultComparer
    function cls.getDefault()
        local comparer = defaultComparer 
        if comparer == nil then
             comparer = System.Comparer_1(T)()
             defaultComparer = comparer
        end
        return comparer
    end
    return cls
end, Comparer)

local function compareT(this, other, T)
    return System.Comparer_1(T).getDefault().compare(this, other)
end

System.compareT = compareT



