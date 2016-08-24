local System = System
local throw = System.throw
local Collection = System.Collection
local removeAtArray = Collection.removeAtArray
local getArray = Collection.getArray

local InvalidOperationException = System.InvalidOperationException

local Queue = {}

function Queue.__ctor__(this, ...)
    local len = select("#", ...)
    if len == 0 then return end
    local collection = ...
    if type(collection) == "number" then return end
    Collection.insertRangeArray(this, 0, collection)
end

function Queue.getCount(this)
    return #this
end

Queue.clear = Collection.removeArrayAll
Queue.enqueue = Collection.pushArray
Queue.getEnumerator = Collection.arrayEnumerator
Queue.contains = Collection.contains
Queue.toArray = Collection.toArray
Queue.trimExcess = System.emptyFn

local function peek(t)
    if #t == 0 then
        throw(InvalidOperationException())
    end
    return getArray(t, 0)
end

Queue.peek = peek

function Queue.dequeue(t)
    local v = peek(t)
    removeAtArray(t, 0)
    return v
end

System.define("System.Queue", function(T) 
    local cls = {
        __inherits__ = { System.IEnumerable_1(T), System.ICollection },
        __genericT__ = T,
    }
    return cls
end, Queue)
