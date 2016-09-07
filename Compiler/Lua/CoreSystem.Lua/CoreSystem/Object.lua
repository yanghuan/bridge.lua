local System = System
local throw = System.throw
local NullReferenceException = System.NullReferenceException

local type = type

local Object = {}

local getHashCodeTable = {
    number = System.Double.GetHashCode,
    string = System.String.GetHashCode ,
    boolean = System.Boolean.GetHashCode,
}

function Object.GetHashCode(this)
    if this == nil then
        throw(NullReferenceException()) 
    end
    local typename = type(this)
    local getHashCode = getHashCodeTable[typename]
    if getHashCode ~= nil then
        return getHashCode(this)
    end
    if typename == "table" then
        getHashCode = this.GetHashCode
        if getHashCode ~= nil then
            return getHashCode(this)     
        end
    end
    return this
end

local equalsObjTable = {
    number = System.Double.EqualsObj,
    string = System.String.EqualsObj ,
    boolean = System.Boolean.EqualsObj,
}

local function equals(this, v)
    local typename = type(this)
    local equals = equalsObjTable[typename]
    if equals ~= nil then
        return equals(this, v)
    end
    if typename == "table" then
        equals = this.EqualsObj
        if equals ~= nil then
            return equals(this, v) 
        end
    end
    return this == v
end

function Object.equals(this, v)
    if this == nil then
        throw(NullReferenceException()) 
    end
    return equals(this, v)
end

function Object.EqualsStatic(x, y)
    if x ~= nil then
        if y ~= nil then return equals(x, y) end
        return false
    end
    if y ~= nil then return false end
    return true
end

function Object.ToString(this)
    if this == nil then
       throw(NullReferenceException()) 
    end
    return tostring(this)
end

function Object.ReferenceEquals(a, b)
    return a == b
end

System.define("System.Object", Object)
