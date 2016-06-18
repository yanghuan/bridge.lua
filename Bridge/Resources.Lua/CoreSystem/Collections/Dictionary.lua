local System = System
local new = System.new
local throw = System.throw
local Collection = System.Collection
local wrap = Collection.wrap
local unWrap = Collection.unWrap
local changeVersion = Collection.changeVersion
local addCount = Collection.addCount
local clearCount = Collection.clearCount
local ArgumentNullException = System.ArgumentNullException
local ArgumentException = System.ArgumentException
local KeyNotFoundException = System.KeyNotFoundException
local EqualityComparer_1 = System.EqualityComparer_1

local Dictionary = {}

local function buildFromCapacity(this, capacity, comparer)
    if comparer ~= nil then
        assert(false)
    end
end

local function buildFromDictionary(this, dictionary, comparer)
    buildFromCapacity(this, 0, comparer)
    if dictionary == nil then
        throw(ArgumentNullException("dictionary"))
    end
    local count = 0
    for k, v in pairs(dictionary) do
        this[k] = v
        count = count + 1
    end
    addCount(this, count)
end

function Dictionary.__ctor__(this, ...) 
    local len = select("#", ...)
    if len == 0 then
        buildFromCapacity(this, 0)
    elseif len == 1 then
        local comparer = ...
        if comparer == nil or type(comparer) == "number" then  
            buildFromCapacity(this, comparer)
        else
            local getHashCode = comparer.getHashCode
            if getHashCode == nil then
                buildFromDictionary(this, comparer)
            else
                buildFromCapacity(this, 0, comparer)
            end
        end
    else 
        local dictionary, comparer = ...
        if type(dictionary) == "number" then 
            buildFromCapacity(this, dictionary, comparer)
        else
            buildFromDictionary(this, dictionary, comparer)
        end
    end
end 

local function checkKey(key)
    if key == nil then
        throw(ArgumentNullException("key"))
    end
end

function Dictionary.add(this, key, value)
    checkKey(key)
    if this[key] then
        throw(ArgumentException("key already exists"))
    end
    this[key] = wrap(value)
    addCount(this, 1)
    changeVersion(this)
end

function Dictionary.clear(this)
    for k, v in pairs(this) do
        this[k] = nil
    end
    clearCount(this)
    changeVersion(this)
end

function Dictionary.containsKey(this, key)
    checkKey(key)
    return this[key] ~= nil 
end

function Dictionary.containsValue(this, value)
    if value == nil then
        for _, v in pairs(this) do
            if unWrap(v) == nil then
                return true
            end
        end    
    else    
        local c = EqualityComparer_1(this.__genericTValue__).getDefault()
        for _, v in pairs(this) do
            if c.equals(value, unWrap(v)) then
                return true
            end
        end
    end
    return false
end

function Dictionary.remove(this, key)
    checkKey(key)
    if this[key] then
        this[key] = nil
        addCount(this, -1)
        changeVersion(this)
        return true
    end
    return false
end

local function getValueDefault(this)
    return this.__genericTValue__.__defaultVal__
end

function Dictionary.tryGetValue(this, key)
    checkKey(key)
    local value = this[key]
    if value == nil then
        return false, getValueDefault(this)
    end
    return true, unWrap(value)
end

function Dictionary.getComparer(this)
    return EqualityComparer_1(this.__genericTKey__).getDefault()
end

Dictionary.getCount = Collection.getCount

function Dictionary.getItem(this, key)
    checkKey(key)
    local value = this[key]
    if value == nil then
        throw(KeyNotFoundException())
    end
    return unWrap(value) 
end

function Dictionary.setItem(this, key, value)
    checkKey(key)
    this[key] = wrap(value)
    changeVersion(this)
end

Dictionary.get = Dictionary.getItem
Dictionary.set = Dictionary.setItem

function Dictionary.getEnumerator(this)
    return Collection.dictionaryEnumerator(this, 0)
end 

local DictionaryCollection = {}
DictionaryCollection.__index = DictionaryCollection

function DictionaryCollection.__ctor__(this, dict, isKey)
    this.dict = dict
    this.isKey = isKey
end

function DictionaryCollection.getCount(this)
    return #this.dict
end

function DictionaryCollection.getEnumerator(this)
    return Collection.DictionaryEnumerator(this.dict, this.isKey and 1 or 2)
end

function DictionaryCollection.contains(this, v)
    if this.isKey then
        return this.dict.containsKey(v)
    end 
    return this.dict.containsValue(v)
end

function Dictionary.getKeys(this)
    return new(DictionaryCollection, this, true)
end

function Dictionary.getValues(this)
    return new(DictionaryCollection, this, false)
end

System.define("System.Dictionary", function(TKey, TValue) 
   local cls = { 
    __inherits__ = { System.IDictionary_2(TKey, TValue) }, 
    __genericTKey__ = TKey,
    __genericTValue__ = TValue,
    __len = Dictionary.getCount
    }
    return cls
end, Dictionary)




