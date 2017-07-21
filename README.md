***Note: Bridge.lua is obsolete and no longer maintained, please use [CSharp.lua](https://github.com/yanghuan/CSharp.lua) instead.***

# Bridge.lua
Bridge.lua is a C#-to-Lua Compiler,which generates equivalent and consistent lua code, it will do some optimizations, such as local optimization, constant conversion, etc. Based on modified from [bridge.net](https://github.com/bridgedotnet/Bridge)

## Try Live
https://yanghuan.github.io/external/bridgelua-editor/index.html

## Sample
The following C# code that implements timer management, reference [folly‘s TimeoutQueue](https://github.com/facebook/folly/blob/master/folly/TimeoutQueue.h)

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ice.Utils {
    public sealed class TimeoutQueue {
        private sealed class Event {
            public int Id;
            public Int64 Expiration;
            public Int64 RepeatInterval;
            public Action<int, Int64> Callback;
            public LinkedListNode<Event> LinkNode;
        }

        private int nextId_ = 1;
        private Dictionary<int, Event> ids_ = new Dictionary<int, Event>();
        private LinkedList<Event> events_ = new LinkedList<Event>();

        private int NextId {
            get { return nextId_++; }
        }

        private void Insert(Event e) {
            ids_.Add(e.Id, e);
            Event next = events_.FirstOrDefault(i => i.Expiration > e.Expiration);
            if(next != null) {
                e.LinkNode = events_.AddBefore(next.LinkNode, e);
            } else {
                e.LinkNode = events_.AddLast(e);
            }
        }

        public int Add(Int64 now, Int64 delay, Action<int, Int64> callback) {
            return AddRepeating(now, delay, 0, callback);
        }

        public int AddRepeating(Int64 now, Int64 interval, Action<int, Int64> callback) {
            return AddRepeating(now, interval, interval, callback);
        }

        public int AddRepeating(Int64 now, Int64 delay, Int64 interval, Action<int, Int64> callback) {
            int id = NextId;
            Insert(new Event() {
                Id = id,
                Expiration = now + delay,
                RepeatInterval = interval,
                Callback = callback
            });
            return id;
        }

        public Int64 NextExpiration {
            get {
                return events_.Count > 0 ? events_.First.Value.Expiration : Int64.MaxValue;
            }
        }

        public bool Erase(int id) {
            Event e;
            if(ids_.TryGetValue(id, out e)) {
                ids_.Remove(id);
                events_.Remove(e.LinkNode);
                return true;
            }
            return false;
        }

        public Int64 RunOnce(Int64 now) {
            return RunInternal(now, true);
        }

        public Int64 RunLoop(Int64 now) {
            return RunInternal(now, false);
        }

        public bool IsEmpty {
            get {
                return ids_.Count == 0;
            }
        }

        public bool Contains(int id) {
            return ids_.ContainsKey(id);
        }

        private Int64 RunInternal(Int64 now, bool onceOnly) {
            Int64 nextExp;
            do {
                List<Event> expired = events_.TakeWhile(i => i.Expiration <= now).ToList();
                foreach(Event e in expired) {
                    Erase(e.Id);
                    if(e.RepeatInterval > 0) {
                        e.Expiration += e.RepeatInterval;
                        Insert(e);
                    }
                }
                foreach(Event e in expired) {
                    e.Callback(e.Id, now);
                }
                nextExp = NextExpiration;
            } while(!onceOnly && nextExp <= now);
            return nextExp;
        }
    }
}
```
You will get the equivalent, the possibility of a good lua code.
```lua
local System = System
local Linq = System.Linq.Enumerable
local IceUtilsTimeoutQueue

System.usingDeclare(function()
    IceUtilsTimeoutQueue = Ice.Utils.TimeoutQueue
end)

System.namespace("Ice.Utils", function(namespace)
    namespace.class("TimeoutQueue", function ()
        local getNextId, getNextExpiration, getIsEmpty, Insert, Add, AddRepeating, AddRepeating_1, Erase
        local RunOnce, RunLoop, Contains, RunInternal
        local __init__, __ctor1__
        __init__ = function (this) 
            this.ids_ = System.Dictionary(System.Int, IceUtilsTimeoutQueue.Event)()
            this.events_ = System.LinkedList(IceUtilsTimeoutQueue.Event)()
        end
        __ctor1__ = function (this) 
            __init__(this)
        end
        getNextId = function (this) 
            local __t__
            __t__ = this.nextId_
            this.nextId_ = this.nextId_ + 1
            return __t__
        end
        getNextExpiration = function (this) 
            local __t__
            if this.events_:getCount() > 0 then
                __t__ = this.events_:getFirst().Value.Expiration
            else
                __t__ = 9223372036854775807
            end
            return __t__
        end
        getIsEmpty = function (this) 
            return this.ids_:getCount() == 0
        end
        Insert = function (this, e) 
            this.ids_:Add(e.Id, e)
            local next = Linq.FirstOrDefault(this.events_, function (i) 
                return i.Expiration > e.Expiration
            end)
            if next ~= nil then
                e.LinkNode = this.events_:AddBefore(next.LinkNode, e)
            else
                e.LinkNode = this.events_:AddLast(e)
            end
        end
        Add = function (this, now, delay, callback) 
            return AddRepeating_1(this, now, delay, 0, callback)
        end
        AddRepeating = function (this, now, interval, callback) 
            return AddRepeating_1(this, now, interval, interval, callback)
        end
        AddRepeating_1 = function (this, now, delay, interval, callback) 
            local id = getNextId(this)
            Insert(this, System.merge(IceUtilsTimeoutQueue.Event(), function (t)
                t.Id = id
                t.Expiration = now + delay
                t.RepeatInterval = interval
                t.Callback = callback
            end))
            return id
        end
        Erase = function (this, id) 
            local __t__
            local e
            __t__, e = this.ids_:TryGetValue(id, e)
            if __t__ then
                this.ids_:Remove(id)
                this.events_:Remove(e.LinkNode)
                return true
            end
            return false
        end
        RunOnce = function (this, now) 
            return RunInternal(this, now, true)
        end
        RunLoop = function (this, now) 
            return RunInternal(this, now, false)
        end
        Contains = function (this, id) 
            return this.ids_:ContainsKey(id)
        end
        RunInternal = function (this, now, onceOnly) 
            local nextExp
            repeat 
                local expired = Linq.ToList(Linq.TakeWhile(this.events_, function (i) 
                    return i.Expiration <= now
                end))
                for _, e in System.each(expired) do
                    Erase(this, e.Id)
                    if e.RepeatInterval > 0 then
                        e.Expiration = e.Expiration + e.RepeatInterval
                        Insert(this, e)
                    end
                end
                for _, e in System.each(expired) do
                    e.Callback(e.Id, now)
                end
                nextExp = getNextExpiration(this)
            until not(not onceOnly and nextExp <= now)
            return nextExp
        end
        return {
            nextId_ = 1,
            __ctor__ = __ctor1__,
            getNextExpiration = getNextExpiration,
            getIsEmpty = getIsEmpty,
            Add = Add,
            AddRepeating = AddRepeating,
            AddRepeating_1 = AddRepeating_1,
            Erase = Erase,
            RunOnce = RunOnce,
            RunLoop = RunLoop,
            Contains = Contains
        }
    end)
    namespace.class("TimeoutQueue.Event", function ()
        local __init__, __ctor1__
        __init__ = function (this) 
            this.Expiration = 0
            this.RepeatInterval = 0
        end
        __ctor1__ = function (this) 
            __init__(this)
        end
        return {
            Id = 0,
            __ctor__ = __ctor1__
        }
    end)
end)
```

## How to Use
### Command Line Parameters
```cmd
D:\bridge>Bridge.Lua.exe -h
Usage: Bridge.Lua [-s srcfolder] [-d dstfolder]
Arguments 
-s              : source directory, all *.cs files whill be compiled
-d              : destination  directory, will put the out lua files

Options
-l              : libraries referenced, use ';' to separate      
-m              : meta files, like System.xml, use ';' to separate     
-h              : show the help message    
-def            : defines name as a conditional symbol, use ';' to separate     
```
### Download
[bridge.lua.1.0.1.zip](https://raw.githubusercontent.com/sy-yanghuan/bridge.lua/master/download/bridge.lua.1.0.1.zip)

## CoreSystem.lua
[CoreSystem.lua library](https://github.com/sy-yanghuan/bridge.lua/tree/master/Compiler/Lua/CoreSystem.Lua/CoreSystem) that implements most of the [.net framework core classes](http://referencesource.microsoft.com/), including support for basic type, delegate, generic collection classes & linq. The Converted lua code, need to reference it  
- Download [CoreSysrem.lua.zip](https://raw.githubusercontent.com/sy-yanghuan/bridge.lua/master/download/CoreSystem.lua.zip)

## Example
- [fibonacci](https://github.com/sy-yanghuan/bridge.lua/tree/master/Example/fibonacci), a console program code, print Fibonacci number. 

## Documentation
- Documentation Home [English](https://github.com/sy-yanghuan/bridge.lua/wiki/English-Home-Page) [Chinese](https://github.com/sy-yanghuan/bridge.lua/wiki/%E4%B8%AD%E6%96%87%E6%96%87%E6%A1%A3)
- FAQ [English](https://github.com/sy-yanghuan/bridge.lua/wiki/FAQ) [Chinese](https://github.com/sy-yanghuan/bridge.lua/wiki/%E5%B8%B8%E8%A7%81%E9%97%AE%E9%A2%98%E8%A7%A3%E7%AD%94)

## CSharp.lua
[CSharp.lua](https://github.com/yanghuan/CSharp.lua) is better and more concise. Bridge.lua is obsolete.

## License
Bridge.lua is released under the [Apache 2.0 license](LICENSE).

## *Thanks*
[http://bridge.net](http://bridge.net )

