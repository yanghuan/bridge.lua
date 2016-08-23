local System = System
local throw = System.throw
local Collection = System.Collection
local changeVersion = Collection.changeVersion
local addCount = Collection.addCount
local clearCount = Collection.clearCount
local each = Collection.each
local ArgumentNullException = System.ArgumentNullException
local InvalidOperationException = System.InvalidOperationException
local EqualityComparer_1 = System.EqualityComparer_1

local LinkedListNode = {}
LinkedListNode.__index = LinkedListNode

local LinkedList = {}

function LinkedList.__ctor__(this, ...)
    local len = select("#", ...)
    if len == 0 then
    else
        local collection = ...
        if collection == nil then
            throw(ArgumentNullException("collection"))
        end
        for _, item in each(collection) do
            this:addLast(item)
        end
    end
end

LinkedList.getCount = Collection.getCount

function LinkedList.getFirst(this)    
    return this.head
end

function LinkedList.getLast(this)
    local head = this.head
    return head ~= nil and head.prev or nil
end

local function vaildateNode(this, node)
    if node == nil then
        throw(ArgumentNullException("node"))
    end
    if node.list ~= this then
        throw(InvalidOperationException("ExternalLinkedListNode"))
    end
end

local function insertNodeBefore(this, node, newNode)
    newNode.next = node
    newNode.prev = node.prev
    node.prev.next = newNode
    node.prev = newNode
    changeVersion(this)
    addCount(this, 1)
end

local function insertNodeToEmptyList(this, newNode)
    newNode.next = newNode
    newNode.prev = newNode
    this.head = newNode
    changeVersion(this)
    addCount(this, 1)
end

local function newLinkedListNode(list, value)
    return setmetatable({ list = list, item = value }, LinkedListNode)
end

function LinkedList.addAfter(this, node, newNode)    
    vaildateNode(this, node)
    if getmetatable(newNode) == LinkedListNode then
        vaildateNode(this, newNode)
        insertNodeBefore(this, node.next, newNode)
        newNode.list = this
    else
        local result = newLinkedListNode(node.list, newNode)
        insertNodeBefore(this, node.next, result)
        return result
    end
end

function LinkedList.addBefore(this, node, newNode)
    vaildateNode(this, node)
    if getmetatable(newNode) == LinkedListNode then
        vaildateNode(this, newNode)
        insertNodeBefore(this, node, newNode)
        newNode.list = this
        if node == this.head then
            this.head = newNode
        end
    else
        local result = newLinkedListNode(node.list, newNode)
        insertNodeBefore(this, node, result)
        if node == this.head then
            this.head = result
        end
        return result
    end
end

function LinkedList.addFirst(this, node)
    if getmetatable(node) == LinkedListNode then
        vaildateNode(this, node)
        if this.head == nil then
            insertNodeToEmptyList(this, node)
        else
            insertNodeBefore(this, this.head, node)
            this.head = node
        end
        node.list = this
    else
        local result = newLinkedListNode(this, node)
        if this.head == nil then
            insertNodeToEmptyList(this, result)
        else
            insertNodeBefore(this, this.head, result)
            this.head = result
        end
        return result
    end
end

function LinkedList.addLast(this, node)
    if getmetatable(node) == LinkedListNode then
        vaildateNode(this, node)
        if this.head == nil then
            insertNodeToEmptyList(this, node)
        else
            insertNodeBefore(this, this.head, node)
        end
        node.list = this
    else
        local result = newLinkedListNode(this, node)
        if this.head == nil then
            insertNodeToEmptyList(this, result)
        else
            insertNodeBefore(this, this.head, result)
        end
        return result
    end
end

local function invalidate(this)
    this.list = nil
    this.next = nil
    this.prev = nil
end

function LinkedList.clear(this)
    local current = this.head
    while current ~= nil do
        local temp = current
        current = current.next
        invalidate(temp)
    end
    this.head = nil
    clearCount(this)
    changeVersion(this)
end

function LinkedList.contains(this, value)
    return this:find(value) ~= nil
end

function LinkedList.find(this, value)     
    local head = this.head
    local node = head
    local equals = EqualityComparer_1(this.__genericT__).getDefault().equals
    if node ~= nil then
         if value ~= nil then
             repeat
                 if equals(node.item, value) then
                     return node
                 end
                 node = node.next
             until node == head
         else
            repeat 
                if node.item == nil then
                    return node
                end
                node = node.next
            until node == head
         end
    end
    return nil
end

function LinkedList.findLast(this, value)
    local head = this.head
    if head == nil then return nil end
    local last = head.prev
    local node = last
    local equals = EqualityComparer_1(this.__genericT__).getDefault().equals
    if node ~= nil then
        if value ~= nil then
            repeat
                if equals(node.item, value) then
                    return node
                end
                node = node.prev
            until node == head
        else
           repeat 
               if node.item == nil then
                   return node
               end
               node = node.prev
           until node == head
        end
   end
   return nil
end

local function remvoeNode(this, node)
    if node.next == node then
        this.head = nil
    else
        node.next.prev = node.prev
        node.prev.next = node.next
        if this.head == node then
            this.head = node.next
        end
    end
    invalidate(node)
    addCount(this, - 1)
    changeVersion(this)
end

function LinkedList.remove(this, node)
    if getmetatable(node) == LinkedListNode then
        vaildateNode(this, node)
        remvoeNode(this, node)
    else
        node = this:find(node)
        if node ~= nil then
            remvoeNode(this, node)
        end
        return false
    end
end

function LinkedList.removeFirst(this)
    local head = this.head
    if head == nil then
        throw(InvalidOperationException("LinkedListEmpty"))
    end
    remvoeNode(this, head)
end

function LinkedList.removeLast(this)
    local head = this.head
    if head == nil then
        throw(InvalidOperationException("LinkedListEmpty"))
    end
    remvoeNode(this, head.prev)
end

function LinkedList.getEnumerator(this)
    return Collection.LinkedListEnumerator(this)
end

System.define("System.LinkedList", function(T) 
   local cls = { 
    __inherits__ = { System.ICollection_1(T), System.ICollection }, 
    __genericT__ = T,
    __len = LinkedList.getCount
    }
    return cls
end, LinkedList)
