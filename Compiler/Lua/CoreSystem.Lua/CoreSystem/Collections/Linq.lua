local System = System
local throw = System.throw
local each = System.each
local Collection = System.Collection
local wrap = Collection.wrap
local unWrap = Collection.unWrap
local NullReferenceException = System.NullReferenceException
local ArgumentNullException = System.ArgumentNullException
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local InvalidOperationException = System.InvalidOperationException
local EqualityComparer_1 = System.EqualityComparer_1
local Comparer_1 = System.Comparer_1

local select = select
local tinser = table.insert
local getmetatable = getmetatable

local Enumerable = {}
Enumerable.__index = Enumerable

local function createInternal(T, getEnumerator)
    assert(T)
    return setmetatable({ __genericT__ = T, getEnumerator = getEnumerator }, Enumerable)
end

local function create(source, getEnumerator)
    return createInternal(source.__genericT__, getEnumerator)
end 

local function IEnumerator(source, tryGetNext, init)
    local state = 1
    local current
    local en
    return {
        moveNext = function()
            if state == 1 then
                state = 2
                if source then
                    en = source:getEnumerator() 
                end
                if init then
                   init(en) 
                end
            end
            if state == 2 then
                local ok, v = tryGetNext(en)
                if ok then
                    current = v
                    return true
                elseif en then
                    local dispose = en.dispose
                    if dispose then
                        dispose(en)
                    end    
                end
            end
            return false
        end,
        getCurrent = function()
            return current
        end
    }
end

local function sampleNext(en)
    if en:moveNext() then
      return true, en:getCurrent()
    end
    return false
end

local function from(_, source)
    if source == nil then throw(ArgumentNullException("source")) end
    if getmetatable(source) == Enumerable then
        return source
    end
    return create(source, function() 
        return IEnumerator(source, sampleNext)
    end)
end

System.Linq = setmetatable({}, { __call = from })

function Enumerable.where(source, predicate)
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    return create(source, function() 
        local index = -1
        return IEnumerator(source, function(en)
            while en:moveNext() do
                local current = en:getCurrent()
                index = index + 1
                if predicate(current, index) then
                    return true, current
                end
            end 
            return false
        end)
    end)
end

function Enumerable.select(source, selector, T)
    if selector == nil then throw(ArgumentNullException("selector")) end
    return createInternal(T, function()
        local index = -1
        return IEnumerator(source, function(en) 
            if en:moveNext() then
                index = index + 1
                return true, selector(en:getCurrent(), index)
            end
            return false
        end)
    end)
end

local function selectMany(source, collectionSelector, resultSelector, T)
    if collectionSelector == nil then throw(ArgumentNullException("collectionSelector")) end
    if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
    return createInternal(T, function() 
        local midEn
        local index = -1
        return IEnumerator(source, function(en) 
            while true do
                if midEn and midEn:moveNext() then
                    if resultSelector ~= nil then
                        return true, resultSelector(midEn, midEn:getCurrent())
                    end
                    return true, midEn:getCurrent()
                else
                    if not en:moveNext() then return false end
                    index = index + 1
                    midEn = collectionSelector(en:getCurrent(), index):getEnumerator()
                    if midEn == nil then
                        throw(NullReferenceException())
                    end
                end  
            end
        end)
    end)
end

function Enumerable.selectMany(source, ...)
    local function identityFn(s, x)
        return x
    end

    local len = select("#", ...)
    if len == 2 then
        local collectionSelector, T = ...
        return selectMany(source, collectionSelector, identityFn, T)
    else
        return selectMany(source, ...)
    end
end

function Enumerable.take(source, count)
    return create(source, function()
        return IEnumerator(source, function(en)
            if count > 0 then
                if en:moveNext() then
                    count = count - 1
                    return true, en:getCurrent()
                end
            end
            return false
        end)
    end)
end

function Enumerable.takeWhile(source, predicate)
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    return create(source, function()
        local index = -1
        return IEnumerator(source, function(en)
            if en:moveNext() then
                local current = en:getCurrent()
                index = index + 1
                if not predicate(current, index) then
                    return false
                end
                return true, current
            end
            return false
        end)
    end)
end

function Enumerable.skip(source, count)
    return create(source, function()
        return IEnumerator(source, function(en)
            while count > 0 and en:moveNext() do count = count - 1 end
            if count <= 0 then
                if en:moveNext() then
                    return true, en:getCurrent() 
                end
            end
            return false
        end)
    end)
end

function Enumerable.skipWhile(source, predicate)
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    return create(source, function()
        local index = -1
        local isSkipEnd = false
        return IEnumerator(source, function(en)
            while not isSkipEnd do
                if en:moveNext() then
                    local current = en:getCurrent()
                    index = index + 1
                    if not predicate(current, index) then
                        isSkipEnd = true
                        return true, current
                    end     
                else 
                    return false
                end
            end
            if en:moveNext() then
                return true, en:getCurrent()
            end
            return false
        end)
    end)
end

local Lookup = setmetatable({}, Enumerable)

function Lookup.__ctor__(this, comparer)
    this.comparer = comparer or EqualityComparer_1(this.__genericTKey__).getDefault()
    this.groups = {}
    this.count = 0
end

function Lookup.getEnumerator(this)
    return Collection.dictionaryEnumerator(this.groups, 2)
end

System.define("System.Linq.Lookup", function(TKey, TElement)
    local cls = {
        __genericTKey__ = TKey,
        __genericTElement__ = TElement,
    }
    return cls
end, Lookup)
System.defInf("System.Linq.IGrouping")

local LookupFn = System.Linq.Lookup
local IGrouping = System.Linq.IGrouping

local Grouping = setmetatable({}, Enumerable)
Grouping.__index = Grouping
Grouping.getEnumerator = Collection.arrayEnumerator 
Grouping.getKey = function(this)
    return this.key
end

local function addToLookup(this, key, value)
    key = this.comparer.getHashCode(key)
    local group = this.groups[key]
    if group == nil then
        group = setmetatable({ key = key, __genericT__ = this.__genericTElement__ }, Grouping)
        this.groups[key] = group        
        this.count = this.count + 1
    end
    tinser(group, wrap(value))
end

local function createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
    local lookup = LookupFn(TKey, TElement)(comparer)
    for _, item in each(source) do
        addToLookup(lookup, keySelector(item), elementSelector(item))
    end
    return lookup
end

local function groupBy(source, keySelector, elementSelector, comparer, TKey, TElement)
    if keySelector == nil then throw(ArgumentNullException("keySelector")) end
    if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
    return createInternal(IGrouping, function()
        return createLookup(source, keySelector, elementSelector, comparer, TKey, TElement):getEnumerator()
    end)
end

local function identityFn(x)
    return x
end

function Enumerable.groupBy(source, ...)
    local len = select("#", ...)
    if len == 2 then
         local keySelector, TKey = ...
         return groupBy(source, keySelector, identityFn, nil, TKey, source.__genericT__)
    elseif len == 3 then
         local keySelector, comparer, TKey = ...
         return groupBy(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
    elseif len == 4 then
         local keySelector, elementSelector, TKey, TElement = ...
         return groupBy(source, keySelector, elementSelector, nil, TKey, TElement)
    else
        return groupBy(source, ...)
    end
end

local function groupBySelect(source, keySelector, elementSelector, resultSelector, comparer, TKey, TElement, TResult)
    if keySelector == nil then throw(ArgumentNullException("keySelector")) end
    if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
    if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
    return createInternal(TResult, function()
        local lookup = createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
        return IEnumerator(lookup, function(en)
            if en:moveNext() then
                local current = en:getCurrent()
                return resultSelector(current.key, current)
            end
            return false
        end, TResult)
    end)
end

function Enumerable.groupBySelect(source, ...)
    local len = select("#", ...)
    if len == 4 then
        local keySelector, resultSelector, TKey, TResult = ...
        return groupBySelect(source, keySelector, identityFn, resultSelector, nil, TKey, source.__genericT__, TResult)
    elseif len == 5 then
        local keySelector, resultSelector, comparer, TKey, TResult = ...
        return groupBySelect(source, keySelector, identityFn, resultSelector, comparer, TKey, source.__genericT__, TResult)
    elseif len == 6 then
        local keySelector, elementSelector, resultSelector, TKey, TElement, TResult = ...
        return groupBySelect(source, keySelector, elementSelector, resultSelector, nil, TKey, TElement, TResult)
    else
        return groupBySelect(source, ...)
    end
end

function Enumerable.concat(first, second)
    if second == nil then throw(ArgumentNullException("second")) end
    return create(first, function()
        local secondEn
        return IEnumerator(first, function(en)
            if secondEn == nil then
                if en:moveNext() then
                    return true, en:getCurrent()
                end
                secondEn =  second:getEnumerator()
            end
            if secondEn:moveNext() then
                return true, secondEn:getCurrent()
            end
            return false
        end)
    end)
end

function Enumerable.zip(first, second, resultSelector, TResult) 
    if second == nil then throw(ArgumentNullException("second")) end
    if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
    return createInternal(TResult, function()
        local e2
        return IEnumerator(first, function(e1)
            if e1:moveNext() and e2:moveNext() then
                return true, resultSelector(e1:getCurrent(), e2:getCurrent())
            end
        end, 
        function()
            e2 = second:getEnumerator()
        end)
    end)
end

local function addToSet(set, v, getHashCode)
    local hashCode = getHashCode(v)
    if set[hashCode] == nil then
        set[hashCode] = true
        return true
    end
    return false
end

local function removeFromSet(set, v, getHashCode)
    local hashCode = getHashCode(v)
    if set[hashCode] ~= nil then
        set[hashCode] = nil
        return true
    end
    return false
end

local function getComparer(source, comparer)
    return comparer or EqualityComparer_1(source.__genericT__).getDefault()
end

function Enumerable.distinct(source, comparer)
    return create(source, function()
        local set = {}
        local getHashCode = getComparer(source, comparer).getHashCode
        return IEnumerator(source, function(en)
            while en:moveNext() do
                local current = en:getCurrent()
                if addToSet(set, current, getHashCode) then
                    return true, current  
                end
            end
            return false
        end)
    end)
end

function Enumerable.union(first, second, comparer)
    if second == nil then throw(ArgumentNullException("second")) end
    return create(first, function()
        local set = {}
        local getHashCode = getComparer(first, comparer).getHashCode
        local secondEn
        return IEnumerator(first, function(en)
            if secondEn == nil then
                while en:moveNext() do
                    local current = en:getCurrent()
                    if addToSet(set, current, getHashCode) then
                        return true, current  
                    end
                end
                secondEn = second:getEnumerator()
            end
            while secondEn:moveNext() do
                local current = secondEn:getCurrent()
                if addToSet(set, current, getHashCode) then
                    return true, current  
                end
            end
            return false
        end)
    end)
end

function Enumerable.intersect(first, second, comparer)
    if second == nil then throw(ArgumentNullException("second")) end
    return create(source, function()
        local set = {}
        local getHashCode = getComparer(first, comparer).getHashCode
        return IEnumerator(first, function(en)
            while en:moveNext() do
                 local current = en:getCurrent()
                 if removeFromSet(set, current, getHashCode) then
                    return true, current
                 end
            end
            return false
        end,
        function()
            for _, v in each(second) do
                addToSet(set, v, getHashCode)
            end
        end)
    end) 
end

function Enumerable.except(first, second, comparer)
    if second == nil then throw(ArgumentNullException("second")) end
    return create(first, function()
        local set = {}
        local getHashCode = getComparer(first, comparer).getHashCode
        return IEnumerator(first, function(en) 
            while en:moveNext() do
                local current = en:getCurrent()
                if addToSet(set, current, getHashCode) then
                    return true, current  
                end
            end
            return false
        end,
        function()
            for _, v in each(second) do
                addToSet(set, v, getHashCode)
            end
        end)
    end)
end

function Enumerable.reverse(source)
    return create(source, function()
        local t = {}    
        local index
        return IEnumerator(nil, function() 
            if index > 1 then
                index = index - 1
                return true, unWrap(t[index])
            end
            return false
        end, 
        function() 
            for _, v in each(source) do
                tinser(t, wrap(v))
            end  
            index = #t + 1
        end)
    end)
end

function Enumerable.sequenceEqual(first, second, comparer)
    if second == nil then throw(ArgumentNullException("second")) end
    local equals = getComparer(first, comparer).equals
    local e1 = first:getEnumerator()
    local e2 = second:getEnumerator()
    while e1:moveNext() do
        if not(e2:MoveNext() and equals(e1:getCurrent(), e2:getCurrent())) then
            return false
        end
    end
    if e2:MoveNext() then
        return false
    end
    return true
end

Enumerable.toArray = Collection.toArray

function Enumerable.toList(source)
    return System.List(source.__genericT__)(source)
end

local function toDictionary(source, keySelector, elementSelector, comparer, TKey, TValue)
    if keySelector == nil then throw(ArgumentNullException("keySelector")) end
    if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
    local dict = System.Dictionary(TKey, TValue)(comparer)
    for _, v in each(source) do
        dict:add(keySelector(v), elementSelector(v))
    end
    return dict
end

function Enumerable.toDictionary(source, ...)
    local len = select("#", ...)
    if len == 2 then
        local keySelector, TKey = ...
        return toDictionary(source, keySelector, identityFn, nil, TKey, source.__genericT__)
    elseif len == 3 then
        local keySelector, comparer, TKey = ...
        return toDictionary(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
    elseif len == 4 then
        local keySelector, elementSelector, TKey, TElement = ...
        return toDictionary(source, keySelector, elementSelector, nil, TKey, TElement)
    else
        return toDictionary(source, ...)
    end
end

local function toLookup(source, keySelector, elementSelector, comparer, TKey, TElement )
    if keySelector == nil then throw(ArgumentNullException("keySelector")) end
    if elementSelector == nil then throw(ArgumentNullException("elementSelector")) end
    return createLookup(source, keySelector, elementSelector, comparer, TKey, TElement)
end

function Enumerable.toLookup(source, ...)
    local len = select("#", ...)
    if len == 2 then
        local keySelector, TKey = ...
        return toLookup(source, keySelector, identityFn, nil, TKey, source.__genericT__)
    elseif len == 3 then
        local keySelector, comparer, TKey = ...
        return toLookup(source, keySelector, identityFn, comparer, TKey, source.__genericT__)
    elseif len == 4 then
        local keySelector, elementSelector, TKey, TElement = ...
        return toLookup(source, keySelector, elementSelector, nil, TKey, TElement)
    else
        return toDictionary(source, ...)
    end
end

local function first(source, ...)
    local len = select("#", ...)
    if len == 0 then  
        local en = source:getEnumerator()
        if en:moveNext() then 
            return true, en:getCurrent()
        end
        return false, 0
    else
        local predicate = ...
        if predicate == nil then throw(ArgumentNullException("predicate")) end
        for _, v in each(source) do
            if predicate(v) then 
                return true, v
            end
        end
        return false, 1
    end
end

function Enumerable.first(source, ...)
    local ok, result = first(source, ...)
    if ok then return result end
    if result == 0 then
        throw(InvalidOperationException("NoElements"))
    end
    throw(InvalidOperationException("NoMatch"))
end

function Enumerable.firstOrDefault(source, ...)
    local ok, result = first(source, ...)
    return ok and result or source.__genericT__.__default__()
end

local function last(source, ...)
    local len = select("#", ...)
    if len == 0 then
        local en = source:getEnumerator()
        if en:moveNext() then 
            local result
            repeat
                result = en:getCurrent()
            until not each.moveNext()
            return true, result
        end
        return false, 0
    else
        local predicate = ...
        if predicate == nil then throw(ArgumentNullException("predicate")) end
        local result, found
        for _, v in each(source) do
            if predicate(v) then
                result = v
                found = true
            end
        end    
        if found then return true, result end
        return false, 1
    end
end

function Enumerable.last(source, ...)
    local ok, result = last(source, ...)
    if ok then return result end
    if result == 0 then
        throw(InvalidOperationException("NoElements"))
    end
    throw(InvalidOperationException("NoMatch"))
end

function Enumerable.lastOrDefault(source, ...)
    local ok, result = last(source, ...)
    return ok and result or source.__genericT__.__default__()
end

local function single(source, ...)
    local len = select("#", ...)
    if len == 0 then
        local en = source:getEnumerator()
        if not en:moveNext() then return false, 0 end
        local result = en:getCurrent()
        if not en:moveNext() then
            return true, result
        end
        return false, 1
    else
        local predicate = ...
        if predicate == nil then throw(ArgumentNullException("predicate")) end
        local result, found
        for _, v in each(source) do
            if predicate(v) then
                result = v
                if found then
                    return false, 1
                end
                found = true
            end
        end
        if foun then return true, result end    
        return false, 0    
    end
end

function Enumerable.single(source, ...)
    local ok, result = single(source, ...)
    if ok then return result end
    if result == 0 then
        throw(InvalidOperationException("NoElements"))
    end
    throw(InvalidOperationException("MoreThanOneMatch"))
end

function Enumerable.singleOrDefault(source, ...)
    local ok, result = single(source, ...)
    return ok and result or source.__genericT__.__default__()
end

local function elementAt(source, index)
    if index < 0 then return false end
    local en = source:getEnumerator()
    while true do
        if not en:moveNext() then return false end
        if index == 0 then return true, en:getCurrent() end
        index = index - 1
    end
end

function Enumerable.elementAt(source, index)
    local ok, result = elementAt(source, index)
    if ok then return result end
    throw(ArgumentOutOfRangeException("index"))
end

function Enumerable.elementAtOrDefault(source, index)
    local ok, result = elementAt(source, index)
    return ok and result or source.__genericT__.__default__()
end

function Enumerable.range(start, count)
    if count < 0 then throw(ArgumentOutOfRangeException("count")) end
    return createInternal(System.Int, function()
        local index = -1
        return IEnumerator(nil, function()
            index = index + 1
            if index < count then
                return true, start + index  
            end
            return false
        end)
    end)
end

function Enumerable.repeat_(element, count, T)
    if count < 0 then throw(ArgumentOutOfRangeException("count")) end
    return createInternal(T, function()
        local index = -1
        return IEnumerator(nil, function()
            index = index + 1
            if index < count then
                return true, element  
            end
            return false
        end)
    end)
end

function Enumerable.any(source, ...)
    local len = select("#", ...)
    if len == 0 then
        local en = source:getEnumerator()
        return en:moveNext()
    else
        local predicate = ...
        if predicate == nil then throw(ArgumentNullException("predicate")) end
        for _, v in each(source) do
            if predicate(v) then
                return true
            end
        end
        return false
    end
end

function Enumerable.all(source, predicate)
    if predicate == nil then throw(ArgumentNullException("predicate")) end
    for _, v in each(source) do
        if not predicate(v) then
            return false
        end
    end
    return true
end

function Enumerable.count(source, ...)
    local len = select("#", ...)
    if len == 0 then
        local count = 0
        local en = source:getEnumerator()
        while en:moveNext() do 
            count = count + 1 
        end
        return count
    else
        local predicate = ...
        if predicate == nil then throw(ArgumentNullException("predicate")) end
        local count = 0
        for _, v in each(source) do
            if predicate(v) then
                coun = count + 1
            end
        end
        return count
    end
end

function Enumerable.contains(source, value, comparer)
    local equals = getComparer(source, comparer).equals
    for _, v in each(source) do
        if equals(v, value) then
            return true
        end
    end
    return false
end

function Enumerable.aggregate(source, ...)
    local len = select("#", ...);
    if len == 1 then
        local func = ...
        if func == nil then throw(ArgumentNullException("func")) end
        local e = source:getEnumerator()
        if not e:moveNext() then throw(InvalidOperationException("NoElements")) end
        local result = e:getCurrent()
        while e:moveNext() do
            result = func(result, e:getCurrent())
        end
        return result
    elseif len == 2 then
        local seed, func = ...
        if func == nil then throw(ArgumentNullException("func")) end
        local result = seed
        for _, element in each(source) do
            result = func(result, element)
        end
        return result
    else 
        local seed, func, resultSelector = ...;
        if func == nil then throw(ArgumentNullException("func")) end
        if resultSelector == nil then throw(ArgumentNullException("resultSelector")) end
        local result = seed
        for _, element in each(source) do
            result = func(result, element)
        end
        return resultSelector(result)
    end
end

function Enumerable.sum(source, ...)
    local len = select("#", ...)
    if len == 0 then
        local sum = 0
        for _, v in each(source) do
            sum = sum + v
        end
        return sum
    else
        local selector = ...
        if selector == nil then throw(ArgumentNullException("selector")) end
        local sum = 0
        for _, v in each(source) do
            sum = sum + selector(v)
        end
        return sum
    end
end

local function minOrMax(compareFn, source, ...)
    local len = select("#", ...)
    local selector, T 
    if len == 0 then
        selector, T = identityFn, source.__genericT__
    else
        selector, T = ...
        if selector == nil then throw(ArgumentNullException("selector")) end
    end
    local compare = Comparer_1(T).getDefault().compare
    local value = T.__default__()
    if value == nil  then
        for _, x in each(source) do
            if x ~= nil and (value == nil or compareFn(compare, x, value)) then
                value = x
            end 
        end
        return value
    else
        local hasValue = false
        for _, x in each(source) do
            if hasValue then
                if compareFn(compare, x, value) then
                    value = x
                end
            else
                value = x
                hasValue = true
            end
        end
        if hasValue then return value end
        throw(InvalidOperationException("NoElements"))
    end
end

local function minFn(compare, x, y)
    return compare(x, y) < 0
end

function Enumerable.min(source, ...)
    return minOrMax(minFn, source, ...)
end

local function maxFn(compare, x, y)
    return compare(x, y) > 0
end

function Enumerable.max(source, ...)
    return minOrMax(maxFn, source, ...)
end




