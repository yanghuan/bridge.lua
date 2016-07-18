local System = System
local throw = System.throw

local Object = {}

local getHashCodeTable = {
    number = System.Double.getHashCode,
    string = System.String.getHashCode ,
    boolean = System.Boolean.getHashCode,
}

function Object.getHashCode(this)
    if this == nil then
        throw(System.NullReferenceException()) 
    end
    local typename = type(this)
    local getHashCode = getHashCodeTable[typename]
    if getHashCode ~= nil then
        return getHashCode(this)
    end
    if typename == "table" then
        getHashCode = this.getHashCode
        if getHashCode ~= nil then
            return getHashCode(this)     
        end
    end
    return this
end

local equalsObjTable = {
    number = System.Double.equalsObj,
    string = System.String.equalsObj ,
    boolean = System.Boolean.equalsObj,
}

local function equals(this, v)
    if this == nil then
        throw(System.NullReferenceException()) 
    end
    local typename = type(this)
    local equals = equalsObjTable[typename]
    if equals ~= nil then
        return equals(this, v)
    end
    if typename == "table" then
        equals = this.equalsObj
        if equals ~= nil then
            return equals(this, v) 
        end
    end
    return this == v
end

Object.equals = equals

function Object.equalsStatic(x, y)
    if x ~= nil then
        if y ~= nil then return equals(x, y) end
        return false
    end
    if y ~= nil then return false end
    return true
end

function Object.toString(this)
    if this == nil then
       throw(System.NullReferenceException()) 
    end
    return tostring(this)
end

function Object.referenceEquals(a, b)
    return a == b
end

System.define("System.Object", Object)
