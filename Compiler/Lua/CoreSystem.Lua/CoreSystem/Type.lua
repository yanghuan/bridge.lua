local System = System
local throw = System.throw
local Int = System.Int
local Double = System.Double
local ArrayFromTable = System.ArrayFromTable
local NullReferenceException = System.NullReferenceException
local InvalidCastException = System.InvalidCastException

local type = type
local getmetatable = getmetatable
local tinsert = table.insert
local ipairs = ipairs

local Type = {}
local numberType = setmetatable({ c = Double, name = "Number", fullName = "System.Number" }, Type)
local types = {
    [Int] = numberType,
    [Double] = numberType,
}

local function typeof(cls)
    assert(cls)
    local type = types[cls]
    if type == nil then
        type = setmetatable({ c = cls }, Type)
        types[cls] = type
    end
    return type
end

local baseTable = {
    number = Double,
    string = System.String,
    boolean = System.Boolean,
    ["function"] = System.Delegate
}

local function getType(obj)
    if obj == nil then
        throw(NullReferenceException())
    end
    local cls = baseTable[type(obj)]
    if cls == nil then
        cls = getmetatable(obj)
    end
    return typeof(cls)
end

System.getType = getType
System.typeof = typeof

local function isGenericName(name)
    return name:byte(#name) == 93
end

function Type.getIsGenericType(this)
    return isGenericName(this.c.__name__)
end

function Type.getIsEnum(this)
    return this.c.__kind__ == "E"
end

function Type.getName(this)
    local name = this.name
    if name == nil then
        local clsName = this.c.__name__
        local pattern = isGenericName(clsName) and "^.*()%.(.*)%[.+%]$" or "^.*()%.(.*)$"
        name = clsName:gsub(pattern, "%2")
        this.name = name
    end
    return name
end

function Type.getFullName(this)
    local fullName = this.fullName
    if fullName == nil then
        fullName = this.c.__name__
        this.fullName = fullName
    end
    return fullName
end

function Type.getNamespace(this)
    local namespace = this.namespace
    if namespace == nil then
        local clsName = this.c.__name__
        local pattern = isGenericName(clsName) and "^(.*)()%..*%[.+%]$" or "^(.*)()%..*$"
        namespace = clsName:gsub(pattern, "%1")
        this.namespace = namespace
    end
    return namespace
end

local function getBaseType(this)
    local baseType = this.baseType
    if baseType == nil then
        local baseCls = this.c.__base__
        if baseCls ~= nil then
            baseType = typeof(baseCls)
            this.baseType = baseType
        end
    end 
    return baseType
end

Type.getBaseType = getBaseType

local function isSubclassOf(this, c)
    local p = this
    if p == c then
        return false
    end
    while p ~= nil do
        if p == c then
            return true
        end
        p = getBaseType(p)
    end
end

Type.isSubclassOf = isSubclassOf

local function getIsInterface(this)
    return this.c.__kind__ == "I"
end

Type.getIsInterface = getIsInterface

local function getInterfaces(this)
    local interfaces = this.interfaces
    if interfaces == nil then
        local interfacesCls = this.c.__interfaces__
        if interfacesCls ~= nil then
            for _, i in ipairs(interfacesCls) do
                tinsert(interfaces, typeof(i))
            end
            ArrayFromTable(interfaces, Type)
        end
    end
    return interfaces
end

Type.getInterfaces = getInterfaces

local function implementInterface(this, ifaceType)
    local t = this
    while t ~= nil do
        local interfaces = getInterfaces(this)
        if interfaces ~= nil then
            for _, i in ipairs(interfaces) do
                if i == ifaceType or implementInterface(i, ifaceType) then
                    return true
                end
            end
        end
        t = getBaseType(t)
    end
end

local function isAssignableFrom(this, c)
    if c == nil then 
        return false 
    end
    if this == c then 
        return true 
    end
    if isSubclassOf(c, this) then
        return true
    end
    if getIsInterface(this) then
        return implementInterface(c, this)
    end
    return false
end 

Type.isAssignableFrom = isAssignableFrom

function Type.isInstanceOfType(this, obj)
    if obj == nil then
        return false 
    end
    return isAssignableFrom(this, getType(obj));
end

function Type.toString(this)
    return this.c.__name__
end

System.define("System.Type", Type)

function is(obj, cls)
    if obj == nil then return false end
    return isAssignableFrom(getType(obj), typeof(cls))
end

System.is = is

function System.as(obj, cls)
    if obj ~= nil and is(obj, cls) then
        return obj
    end
    return nil
end

function System.cast(obj, type)
    if obj ~= nil and not is(obj, type) then
         throw(InvalidCastException(), 1)
    end
    return obj
end



