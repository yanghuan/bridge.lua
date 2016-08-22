local System = System
local Collection = System.Collection
local buildArray = Collection.buildArray
local checkIndex = Collection.checkIndex 
local arrayEnumerator = Collection.arrayEnumerator

local Array = {}
local emptys = {}

Array.__ctor__ = buildArray
Array.set = Collection.setArray
Array.get = Collection.getArray
Array.getEnumerator = arrayEnumerator

function Array.getLength(this)
    return #this
end

function Array.getRank(this)
   return 1
end

function Array.Empty(T)
    local t = emptys[T]
    if t == nil then
        t = Array(T)(0)
        emptys[T] = t
    end
    return t
end

Array.exists = Collection.existsOfArray
Array.find = Collection.findOfArray

local findAll = Collection.findAllOfArray
function Array.findAll(t, match)
    return findAll(t, match):toArray()
end

Array.findIndex = Collection.findIndexOfArray
Array.findLast = Collection.findLastOfArray
Array.findLastIndex = Collection.findLastIndexOfArray
Array.indexOf = Collection.indexOfArray
Array.lastIndexOf = Collection.lastIndexOfArray
Array.reverse = Collection.reverseArray
Array.sort = Collection.sortArray
Array.trueForAll = Collection.trueForAllOfArray
Array.copy = Collection.copyArray

System.define("System.Array", function(T) 
    local cls = { 
    __inherits__ = { System.ICollection_1(T), System.ICollection, System.IList_1(T) }, 
    __genericT__ = T
    }
    return cls
end, Array)

function System.arrayFromTable(t, T)
    return setmetatable(t, System.Array(T))
end

local MultiArray = {}

function MultiArray.__ctor__(this, rank, ...)
    this.__rank__ = rank
    local length = 1
    for _, i in ipairs(rank) do
        length = length * i
    end
    buildArray(this, length, ...)
end

local function getIndex(this, ...)
    local rank = this.__rank__
    local id = 0
    local len = #rank
    for i = 1, len do
        id = id * rank[i] + select(i, ...)
    end
    return id, len
end

function MultiArray.set(this, ...)
    local index, len = getIndex(this, ...)
    setArray(this, index, select(len + 1, ...))
end

function MultiArray.get(this, ...)
    local index = getIndex(this, ...)
    return getArray(this, index)
end

function MultiArray.getLength(this, dimension)
    if dimension == nil then
        return #this
    end
    local rank = thiss.__rank__
    checkIndex(rank, dimension)
    return this.__rank__[dimension + 1]
end

function MultiArray.getRank(this)
   return #this.__rank__
end

MultiArray.getEnumerator = arrayEnumerator

System.define("System.MultiArray", function(T) 
    local cls = { 
    __inherits__ = { System.ICollection_1(T), System.ICollection, System.IList_1(T) }, 
    __genericT__ = T
    }
    return cls
end, MultiArray)