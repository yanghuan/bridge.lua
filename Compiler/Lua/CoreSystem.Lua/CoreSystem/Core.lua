local bit = require("bit")
--local ok, socket = pcall(require, "socket") 
--local time = ok and socket.gettime or os.time
local time = os.time

local setmetatable = setmetatable
local getmetatable = getmetatable
local type = type
local ipairs = ipairs
local assert = assert
local table = table
local tinsert = table.insert
local tremove = table.remove
local rawget = rawget
local floor = math.floor
local error = error

local emptyFn = function() end
local identityFn = function(x) return x end
local genericCache = {}
local class = {}
local modules = {}
local usings = {}
local id = 0
local Object = {}

local function new(cls, ...)
    local this = setmetatable({}, cls)
    cls.__ctor__(this, ...)
    return this
end

local function throw(e, lv)
    e:traceback(lv)
    error(e)
end

local function try(try, catch, finally)
    local ok, err = pcall(try)
    local rethrow
    if not ok then
        if catch then
            if type(err) == "string" then
                err = System.Exception(err)
            end
            local fine, result = pcall(catch, err)
            if not fine then
                err = result
            else
                rethrow = result
            end
        else 
            rethrow = true
        end
    end
    if finally then
        finally()
    end
    if rethrow then
        throw(err)
    end
end

local function set(className, cls)
    local scope = _G
    local starInx = 1
    while true do
        local pos = className:find("%.", starInx) or 0
        local name = className:sub(starInx, pos -1)
        if pos ~= 0 then
            local t = scope[name]
            if t == nil then
                t = {}
                scope[name] = t
            end
            scope = t
        else
            assert(scope[name] == nil, className)
            scope[name] = cls
            break
        end
        starInx = pos + 1
    end
    return cls
end

local function getId()
    id = id + 1
    return id
end

local function defaultValOfZero()
    return 0
end

local function genericId(id, ...) 
    for i = 1, select("#", ...) do
        local cls = select(i, ...)
        id = id .. "." .. cls.__id__
    end
    return id
end

local function genericName(name, ...)
    name = name .. "["
    local comma
    for i = 1, select("#", ...) do
        local cls = select(i, ...)
        if comma then
            name = name .. "," .. cls.__name__
        else
            name = name .. cls.__name__
            comma = true
        end
    end
    name = name .. "]"
    return name
end

local function def(name, kind, cls, generic)
    if type(cls) == "function" then
        if generic then
            generic.__index = generic
            generic.__call = new
        end
        local id = getId()
        local fn = function(...)
            local trueId = genericId(id, ...)
            local t = genericCache[trueId]
            if t == nil then
                local obj = cls(...) or {}
                t = def(nil, kind, obj, genericName(name, ...))
                if generic then
                    setmetatable(t, generic)
                end
                genericCache[trueId] = t
            end
            return t
        end
        return set(name, setmetatable(generic or {}, { __call = function(_, ...) return fn(...) end, __index = Object }))
    end
    cls = cls or {}
    if name ~= nil then
        set(name, cls)
        cls.__name__ = name
    else
        cls.__name__ = generic
    end
    cls.__kind__ = kind
    cls.__id__ = getId()
    if kind == "C" or kind == "S" then
        cls.__index = cls 
        cls.__call = new
        local extends = cls.__inherits__
        if extends then
            if type(extends) == "function" then
                extends = extends()
            end
            local base = extends[1]
            if base.__kind__ == "C" then
                cls.__base__ = base
                setmetatable(cls, base)
                tremove(extends, 1)
                if #extends > 0 then
                    cls.__interfaces__ = extends
                end
                cls.__inherits__ = nil
                if cls.__ctor__ == nil then
                    local baseCtor = base.__ctor__
                    cls.__ctor__ = type(baseCtor) == "table" and baseCtor[1] or baseCtor
                end 
            else
                setmetatable(cls, Object)
                cls.__interfaces__ = extends
                cls.__inherits__ = nil
            end
        elseif cls ~= Object then
             setmetatable(cls, Object)
        end    
        if cls.__default__ == nil then
            cls.__default__ = emptyFn
        end
        if cls.__ctor__ == nil then
            cls.__ctor__ = emptyFn
        end
        tinsert(class, cls)
    elseif kind == "I" then
        cls.__default__ = emptyFn
    elseif kind == "E" then
        cls.__default__ = defaultValOfZero
    else
        assert(false)
    end
    return cls
end

local function defCls(name, cls, genericSuper)
    return def(name, "C", cls, genericSuper) 
end

local function defInf(name, cls)
    return def(name, "I", cls)
end

local function defStc(name, cls, genericSuper)
    return def(name, "S", cls, genericSuper)
end

System = {
    null = null,
    emptyFn = emptyFn,
    identityFn = identityFn,
    try = try,
    throw = throw,
    define = defCls,
    defInf = defInf,
    defStc = defStc,
}

local System = System

System.bnot = bit.bnot
System.band = bit.band
System.bor = bit.bor
System.xor = bit.bxor
System.sl = bit.lshift
System.sr = bit.rshift
System.srr = bit.arshift

local function trunc(num) 
    return num > 0 and floor(num) or floor(-num)
end

System.trunc = trunc

function System.div(x, y) 
    if y == 0 then
        throw(System.DivideByZeroException())
    end
    return trunc(x / y);
end

function System.mod(x, y) 
    if y == 0 then
        throw(System.DivideByZeroException())
    end
    return x % y;
end

function System.strconcat(t)    
    if t == nil then return "" end
    return t
end

System.time = time
    
function System.getTimeZone()
    local now = os.time()
    return os.difftime(now, os.time(os.date("!*t", now)))
end

function System.using(t, f, ...)
    local dispose = t.dispose
    local ret
    if dispose == nil or dispose == emptyFn then
        ret = f(t, ...)
    else 
        local ok, err = pcall(f, t, ...)
        dispose(t)
        if not ok then
            throw(err)
        else 
            ret = err
        end
    end
    return ret
end

function System.merge(t, f)
    f(t)
    return t
end

function System.CreateInstance(type, ...)
    return type.c(...)
end

function System.getclass(className)
    local scope = _G
    local starInx = 1
    while true do
        local pos = className:find("%.", starInx) or 0
        local name = className:sub(starInx, pos -1)
        if pos ~= 0 then
            local t = scope[name]
            if t == nil then
                return nil
            end
            scope = t
        else
            return scope[name]
        end
        starInx = pos + 1
    end
end

function System.property(t, name, v)
    t[name] = v
    local function get(this)
        return this[name]
    end
    local function set(this, v)
        this[name] = v
    end;
    t["get" .. name] = get
    t["set" .. name] = set
    return get, set
end

function System.event(t, name, v)
    t[name] = v
    local function add(this, v)
        this[name] = System.fn.combine(this[name], v)
    end
    local function remove(this, v)
        this[name] = System.fn.remove(this[name], v)
    end
    t["add" .. name] = add
    t["remove" .. name] = remove
    return add, remove
end

function System.usingDeclare(f)
    tinsert(usings, f)
end

function System.init(namelist)
    for _, name in ipairs(namelist) do
       modules[name](name)
    end
    for _, f in ipairs(usings) do
        f()
    end
    for _, cls in ipairs(class) do
        local staticInit, staticCtor =  cls.__staticInit__, cls.__staticCtor__
        if staticInit then
            staticInit(cls)
            cls.__staticInit__ = nil
        end
        if staticCtor then
            staticCtor(cls)
            cls.__staticCtor__ = nil
        end
    end
    modules = {}
    class = {}
    usings = {}
end

local namespace = {}

local function namespaceDef(kind, name, f)
    name = namespace.name .. name
    assert(modules[name] == nil, name)
    modules[name] = function()
       local t = f()
       def(name, kind, t)
    end
end

function namespace.class(name, f)
    namespaceDef("C", name, f) 
end

function namespace.struct(name, f)
    namespaceDef("S", name, f) 
end

function namespace.interface(name, f)
    namespaceDef("I", name, f) 
end

function namespace.enum(name, f)
    namespaceDef("E", name, f)
end

function System.namespace(name, f)
    if name ~= "" then
        name = name .. '.'
    end
    namespace.name = name
    f(namespace)
end

local function multiNew(cls, inx, ...) 
    local this = setmetatable({}, cls)
    cls.__ctor__[inx](this, ...)
    return this
end

local function equals(a, b)
    return a == b
end

Object.__call = new
Object.new = multiNew
Object.EqualsObj = equals
Object.ReferenceEquals = equals
Object.GetHashCode = identityFn
Object.ToString = tostring

function Object.EqualsStatic(x, y)
    if x == y then
        return true
    end
    if x == nil or y == nil then
        return false
    end
    return x:EqualsObj(y)
end

defCls("System.Object", Object)

