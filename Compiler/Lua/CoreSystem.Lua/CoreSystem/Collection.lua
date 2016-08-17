local System = System
local throw = System.throw
local sr = System.sr
local ArgumentOutOfRangeException = System.ArgumentOutOfRangeException
local InvalidOperationException = System.InvalidOperationException
local ArgumentNullException = System.ArgumentNullException
local EqualityComparer_1 = System.EqualityComparer_1
local Comparer_1 = System.Comparer_1

local tinsert = table.insert
local tremove = table.remove
local setmetatable = setmetatable
local select = select
local type = type
local assert = assert
local coroutine = coroutine

local Collection = {}
local null = {}

local versons = setmetatable({}, { __mode = "k" })

local function getVersion(t)
    return versons[t] or 0
end

local function changeVersion(t)
    local verson = getVersion(t)
    versons[t] = verson + 1
end

local function checkVersion(t, verson)
    if verson ~= getVersion(t) then
        throw(InvalidOperationException("has change when iterator"))
    end 
end

Collection.getVersion = getVersion
Collection.changeVersion = changeVersion
Collection.checkVersion = checkVersion

local counts = setmetatable({}, { __mode = "k" })

local function getCount(t)
    return counts[t] or 0
end

local function addCount(t, inc)
    if inc ~= 0 then
        local v = (counts[t] or 0) + inc
        assert(v >= 0)
        counts[t] = v 
    end
end

local function clearCount(t)
    counts[t] = nil
end

Collection.getCount = getCount
Collection.addCount = addCount
Collection.clearCount = clearCount

local function checkIndex(t, index) 
    if index < 0 or index >= #t then
        throw(ArgumentOutOfRangeException("index"))
    end
end

local function checkIndexAndCount(t, index, count)
    if count < 0 or index > #t - count then
        throw(ArgumentOutOfRangeException("index or count"))
    end
end

Collection.checkIndex = checkIndex
Collection.checkIndexAndCount = checkIndexAndCount

local function wrap(v)
    if v ~= nil then 
        return v 
    end
    return null
end

local function unWrap(v)
    if v ~= null then 
        return v 
    end
    return nil
end

Collection.wrap = wrap
Collection.unWrap = unWrap

function Collection.getArray(t, index)
    checkIndex(t, index)
    local item = t[index + 1] 
    return unWrap(item)
end

function Collection.setArray(t, index, v)
    checkIndex(t, index)
    t[index + 1] = wrap(v)
    changeVersion(t)
end

function Collection.pushArray(t, v)
    tinsert(t, wrap(v))
    changeVersion(t)
end

function Collection.buildArray(t, size, ...)
    local len = select("#", ...)
    for i = 1, len do
        local v = select(i, ...)   
        tinsert(t, wrap(v))
    end
    if len < size then
        local default = t.__genericT__.__defaultVal__
        if default == nil then
            default = null
        end
        for i = len + 1, size do
            tinsert(t, default)
        end
    end
end

function Collection.insertArray(t, index, v)
    checkIndex(t, index)
    tinsert(t, index + 1, wrap(v))
    changeVersion(t)
end

function Collection.removeArrayAll(t)
    for i = #t, 1, -1 do
        tremove(t, i)
    end
    changeVersion(t)
end

function Collection.removeArray(t, index, count)
    checkIndexAndCount(t, index, count)
    for i = index + count, index + 1, -1 do
        tremove(t, i)
    end
    changeVersion(t)
end

function Collection.removeAtArray(t, index)
    checkIndex(t, index)
    tremove(t, index + 1)
    changeVersion(t)
end

local function binarySearchArray(t, index, count, v, comparer)
    checkIndexAndCount(t, index, count)
    if comparer == nil then
        comparer = Comparer_1(t.__genericT__).getDefault().compare 
    elseif type(comparer) ~= "function" then    
        comparer = comparer.compare
    end
    local lo = index
    local hi = index + count - 1
    while lo <= hi do
        local i = lo + sr(hi - lo, 1)
        local order = comparer(unWrap(t[i + 1]), v);
        if order == 0 then return i end
        if order < 0 then
            lo = i + 1
        else
            hi = i - 1
        end
    end
    return -1
end 

function Collection.binarySearchArray(t, ...)
    local v, index, count, comparer
    local len = select("#", ...)
    if len == 1 then
        v = ...
        index = 0
        count = #t
    elseif len == 2 then
        v, index = ...
        count = #t - index
    elseif len == 3 then
        index, count, v = ...
    else
        index, count, v, comparer = ...
    end
    return binarySearchArray(t, index, count, v, comparer)
end

local function findIndexOfArray(t, ...)
    local startIndex, count, match
    local len = select("#", ...)
    if len == 1 then
        startIndex = 0
        count = #t
        match = ...
    elseif len == 2 then
        startIndex, match  = ...
        count = #t - startIndex
    else
        startIndex, count, match = ...
    end
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    checkIndexAndCount(t, startIndex, count)
    local endIndex = startIndex + count
    for i = startIndex + 1, endIndex  do
        local item = unWrap(t[i])
        if match(item) then
            return i - 1
        end
    end
    return -1
end

Collection.findIndexOfArray = findIndexOfArray

function Collection.existsOfArray(t, match)
    return findIndexOfArray(t, match) ~= -1
end

function Collection.findOfArray(t, match)
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    for _, i in ipairs(t) do
        local item = unWrap(i)
        if match(item) then
            return item
        end
    end
    return this.__genericT__.__defaultVal__
end

function Collection.findAllOfArray(t, match)
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    local list = System.List(t.__genericT__)()
      for _, i in ipairs(t) do
        local item = unWrap(i)
        if match(item) then
            list:add(item)
        end
    end
    return list
end

function Collection.findLastOfArray(t, match)
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    for i = #t, 1, -1 do
        local item = unWrap(t[i])
        if match(item) then
            return item
        end
    end
    return t.__genericT__.__defaultVal__
end

function Collection.findLastIndexOfArray(t, ...)
    local startIndex, count, match
    local len = select("#", ...)
    if len == 1 then
        startIndex = 0
        count = #t
        match = ...
    elseif len == 2 then
        startIndex, match  = ...
        count = #t - startIndex
    else
        startIndex, count, match = ...
    end
    if match == nil then
        throw(ArgumentNullException("match"))
    end
    if count < 0 or startIndex - count + 1 < 0 then
        throw(ArgumentOutOfRangeException("count"))
    end
    local endIndex = startIndex - count
    checkIndex(endIndex)
    for i = startIndex + 1, endIndex, -1 do
        local item = unWrap(t[i])
        if match(item) then
            return i - 1
        end
    end
    return -1
end

local function indexOfArray(t, v, index, count)
    checkIndexAndCount(t, index, count)
    local equals = EqualityComparer_1(t.__genericT__).getDefault().equals
    for i = index + 1, index + count do 
        if equals(unWrap(t[i]), v) then
            return i - 1
        end
    end
    return -1
end

function Collection.indexOfArray(t, ...)
    local v, index, count
    local len = select("#", ...)
    if len == 1 then
        v = ...
        index = 0
        count = #t
    elseif len == 2 then
        v, index = ...
        count = #t - index
    else
        v, index, count = ...
    end
    return indexOfArray(t, v, index, count)
end

function Collection.contains(t, item)
    return indexOfArray(t, item, 0, #t) ~= -1
end

local function lastIndexOfArray(t, v, index, count)
    if count < 0 or index - count + 1 < 0 then
        throw(ArgumentOutOfRangeException("count"))
    end
    checkIndex(t, index - count)
    local equals = EqualityComparer_1(t.__genericT__).getDefault().equals
    for i = index + 1, index - count, -1 do 
        if equals(unWrap(t[i]), v) then
            return i - 1
        end
    end
    return -1
end

function Collection.lastIndexOfArray(t, ...)
    local v, index, count
    local len = select("#", ...)
    if len == 1 then
        v = ...
        count = #t
        index = count - 1
    elseif len == 2 then
        v, index = ...
        count = #t == 0 and 0 or (index + 1)
    else
        v, index, count = ...
    end
    return lastIndexOfArray(t, v, index, count)
end

function Collection.reverseArray(t, index, count)
    if not index then
        index = 0
        count = #t
    end 
    checkIndexAndCount(t, index, count)
    local i, j = index + 1, index + count
    while i <= j do
        t[i], t[j] = t[j], t[i]
    end
    changeVersion(t)
end

local function sortArray(t, index, count, comparer)
    if count > 1 then
        checkIndexAndCount(t, index, count)
        if comparer == nil then
            comparer = Comparer_1(t.__genericT__).getDefault().compare 
        elseif type(comparer) ~= "function" then    
            comparer = comparer.compare
        end
        local comp = function(x, y) 
            return comparer(unWrap(x), unWrap(y)) < 0
        end
        if index == 0 and count == #t then
            table.sort(t, comp)
        else
            local arr = {}
            for i = index + 1, index + count do
                tinsert(arr, t[i])
            end
            table.sort(arr, comp)
            for i = index + 1, index + count do
                t[i] = arr[i - index]
            end
        end
        changeVersion(t)
    end
end

function Collection.sortArray(t, ...)
    local index, count, comparer
    local len = select("#", ...)
    if len == 0 then
        index = 0
        count = #t
    elseif len == 1 then
        comparer = ...
        index = 0
        count = #t
    elseif len == 2 then
        index, count = ...
    else
        index, count, comparer = ...
    end
    sortArray(t, index, count, comparer)
end

function Collection.trueForAllOfArray(t, match)
     if match == nil then
        throw(ArgumentNullException("match"))
    end
    for _, i in ipairs(t) do
        if not match(unWrap(i)) then
            return false
        end
    end
    return true
end

local ipairsFn = ipairs(null)

function ipairsArray(t)
    local version = getVersion(t)
    return function(t, inx) 
        checkVersion(t, version)
        local k, v = ipairsFn(t, inx)
        return k, unWrap(v)
    end, t, 0
end

Collection.ipairs = ipairsArray

local pairsFn = next

function Collection.pairs(t)
    local version = getVersion(t)
    return function(t, inx) 
        checkVersion(t, version)
        local k, v = pairsFn(t, inx)
        return k, unWrap(v)
    end, t, nil
end

function Collection.forEachArray(t, action)
    if action == null then
        throw(ArgumentNullException("action"))
    end
    local verson = getVersion(t)
    for _, i in ipairs(t) do
        checkVersion(t, verson)
        action(unWrap(i))
    end
end

local ArrayEnumerator = {}
ArrayEnumerator.__index = ArrayEnumerator

function ArrayEnumerator.moveNext(this)
    local t = this.list
    checkVersion(t, this.verson)
    local index = this.index
    if index < #t then
        local i, v = ipairsFn(t, index)
        this.current = unWrap(v)
        this.index = i
        return true
    end
    this.current = nil
    return false
end

function ArrayEnumerator.getCurrent(this)
    return this.current
end

function ArrayEnumerator.reset(this)
    this.index = 0
    this.current = nil
end

local function arrayEnumerator(t)
    local en = {
        list = t,
        index = 0,
        verson = getVersion(t),
    }
    setmetatable(en, ArrayEnumerator)
    return en
end

Collection.arrayEnumerator = arrayEnumerator

local function isArrayLike(t)
    return t.getEnumerator == arrayEnumerator
end

Collection.isArrayLike = isArrayLike

function Collection.isEnumerableLike(t)
    return type(t) == "table" and t.getEnumerator ~= nil
end

local function eachFn(en)
    if en:moveNext() then
        return true, en:getCurrent()
    end
    return nil
end

local function each(t)
    if isArrayLike(t) then
        return ipairsArray(t)
    end
    local en = t:getEnumerator()
    return eachFn, en
end

Collection.each = each

function Collection.insertRangeArray(t, index, collection) 
    if collection == nil then
        throw(ArgumentNullException("collection"))
    end
    for _, v in each(collection) do
        index = index + 1
        tinsert(t, index, wrap(v))
    end
    changeVersion(t)
end

function Collection.toArray(t)
     local array = {}    
     if isArrayLike(t) then
         for _, v in ipairs(t) do
            tinsert(array, v)
         end   
     else
        for _, v in each(t) do
            tinsert(array, wrap(v))
        end
     end
     return System.arrayFromTable(array, t.__genericT__)
end

local DictionaryEnumerator = {}
DictionaryEnumerator.__index = DictionaryEnumerator

function DictionaryEnumerator.moveNext(this)
    local t = this.dict
    checkVersion(t, this.version)
    local k, v = pairsFn(t, this.index)
    if k ~= nil then
        if this.kind == 0 then
            local pair = this.pair
            pair.key = k
            pair.value = unWrap(v)
            this.current = pair
        elseif this.kind == 1 then
            this.current = unWrap(k)
        else
            this.current = unWrap(v)
        end
        this.index = k
        return true
    end
    this.current = nil
    return false
end

function DictionaryEnumerator.getCurrent(this)
    return this.current
end

function Collection.dictionaryEnumerator(t, kind)
    local en = {
        dict = t,
        version = getVersion(t),
        kind = kind,
        pair = kind == 0 and { key = false, value = false } or nil
    }
    setmetatable(en, DictionaryEnumerator)
    return en
end

local LinkedListEnumerator = {}
LinkedListEnumerator.__index = LinkedListEnumerator

function LinkedListEnumerator.moveNext(this)
    local list = this.list
    local node = this.node
    checkVersion(list, this.version)
    if node == nil then
        return false
    end
    this.current = node.item
    node = node.next
    if node == list.head then
        node = nil
    end
    this.node = node
    return true 
end

function LinkedListEnumerator.getCurrent(this)
    return this.current
end

function Collection.LinkedListEnumerator(t)
    local en = {
        list = t,
        version = getVersion(t),
        node = t.head
    }
    setmetatable(en, LinkedListEnumerator)
    return en
end

local YieldEnumerator = {}
YieldEnumerator.__index = YieldEnumerator

function YieldEnumerator.getEnumerator(this)
    return this
end

function YieldEnumerator.moveNext(this)
    local co = this.co
    if coroutine.status(co) == "dead" then
        this.current = nil
        return false
    else
        local ok, v = coroutine.resume(co)
        if ok then
            if coroutine.status(co) == "dead" then
                this.current = nil
                return false
            end
            this.current = v
            return true
        else
            throw(v)
        end
    end
end

function YieldEnumerator.getCurrent(this)
    return this.current
end

function Collection.yieldEnumerator(f, T)
    return setmetatable({ co = coroutine.create(f), __genericT__ = T }, YieldEnumerator)
end

Collection.yieldReturn = coroutine.yield

System.Collection = Collection
System.each = Collection.each
System.ipairs = Collection.ipairs
System.pairs = Collection.pairs
System.isArrayLike = Collection.isArrayLike
System.isEnumerableLike = Collection.isEnumerableLike
System.yieldEnumerator = Collection.yieldEnumerator
System.yieldReturn = Collection.yieldReturn 
















