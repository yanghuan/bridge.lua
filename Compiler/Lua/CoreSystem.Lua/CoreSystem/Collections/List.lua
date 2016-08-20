local System = System
local throw = System.throw
local foreach = System.foreach
local Collection = System.Collection
local unWrap = Collection.unWrap
local checkIndex = Collection.checkIndex
local checkIndexAndCount = Collection.checkIndexAndCount
local copy = Collection.Copy

local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException

local List = {}

function List.__ctor__(this, ...)
    local len = select("#", ...)
    if len == 0 then return end
    local collection = ...
    if type(collection) == "number" then 
        return 
    end
    this:addRange(collection)
end

function List.getCapacity(this)
    return #this
end

function List.getCount(this)
    return #this
end

function List.getIsFixedSize(this)
    return false
end

function List.getIsReadOnly(this)
    return false
end

local getArray = Collection.getArray
local setArray = Collection.setArray

List.get = getArray
List.set = setArray
List.getItem = getArray
List.setItem = setArray
List.add = Collection.pushArray

function List.addRange(this, collection)
    this:insertRange(0, collection)
end

List.binarySearch = Collection.binarySearchArray
List.clear = Collection.removeArrayAll
List.contains = Collection.contains

function List.copyTo(this, ...)
    local len = select("#", ...)
    if len == 1 then
        local array = ...
        copy(this, 0, array, 0, #this)
    elseif len == 2 then
        local array, arrayIndex = ...
        copy(this, 0, array, arrayIndex, #this)
    end
    local index, array, arrayIndex, count = ...
    copy(this, index, array, arrayIndex, count)
end 

List.exists = Collection.existsOfArray
List.find = Collection.findOfArray
List.findAll = Collection.findAllOfArray
List.findIndex = Collection.findIndexOfArray
List.findLast = Collection.findLastOfArray
List.findLastIndex = Collection.findLastIndexOfArray
List.forEach = Collection.forEachArray
List.getEnumerator = Collection.arrayEnumerator

function List.getRange(this, index, count)
    checkIndexAndCount(this, index, count)
    local list = System.List(this.__genericT__)()
    copy(list, index, this, 0, count)
    return list
end

local indexOf = Collection.indexOfArray
local removeAt = Collection.removeAtArray
local removeArray = Collection.removeArray

List.indexOf = indexOf
List.insert = Collection.insertArray
List.insertRange = Collection.insertRangeArray
List.lastIndexOf = Collection.lastIndexOfArray

function List.remove(this, item)
    local index = indexOf(this, item)
    if index >= 0 then
        removeAt(this, index)
        return true
    end
    return false
end

function List.removeAll(this, match)
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    local size = #this
    local freeIndex = 1
    while freeIndex <= size and (not match(unWrap(this[freeIndex]))) do freeIndex = freeIndex + 1 end
    if freeIndex > size then return 0 end

    local current = freeIndex + 1
    while current <= size do 
        while current <= size and match(unWrap(this[current])) do current = current + 1 end
        if current <= size then
            this[freeIndex] = this[current]
            freeIndex = freeIndex + 1
            current = current + 1
        end
    end
    freeIndex = freeIndex -1
    local count = size - freeIndex
    removeArray(this, freeIndex, count)
    return count
end

List.removeAt = removeAt
List.removeRange = removeArray
List.reverse = Collection.reverseArray
List.sort = Collection.sortArray
List.trimExcess = System.emptyFn
List.toArray = Collection.toArray
List.trueForAll = Collection.trueForAllOfArray

System.define("System.List", function(T) 
    local cls = { 
    __inherits__ = { System.ICollection_1(T), System.ICollection, System.IList_1(T) }, 
    __genericT__ = T,
    }
    return cls
end, List)

