local System = System
local emptyFn  = System.emptyFn

local IEnumerable = { __inherits__ = { System.IEnumerable } } 

System.defInf('System.IEnumerable')
System.defInf('System.IEnumerator')
System.defInf('System.IEqualityComparer')
System.defInf('System.ICollection', IEnumerable)

System.defInf('System.IEnumerator_1', function(T) 
    return IEnumerable
end)

System.defInf('System.IEnumerable_1', function(T) 
    return IEnumerable
end)

System.defInf('System.ICollection_1', function(T) 
    return { 
    __inherits__ = { System.IEnumerable_1(T) } 
    }
end)

System.defInf('System.IEqualityComparer_1', emptyFn)

--[[
System.defInf('System.IDictionary_2', function(TKey, TValue) 
    return {
        __inherits__ = { System.IEnumerable_1(System.KeyValuePair_2(TKey, TValue)) }
    }
end)
--]]

System.defInf('System.IDictionary_2', function(TKey, TValue) 
    return IEnumerable
end)

System.defInf('System.IList_1', function(T) 
    return {
        __inherits__ = { System.ICollection_1(T) }
    }
end)

System.defInf('System.IComparer_1', emptyFn)

System.defInf('System.ISet_1', function(T) 
    return {
        __inherits__ = { System.ICollection_1(T) }
    }
end)