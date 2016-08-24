local System = System
local throw = System.throw
local div = System.div
local ArgumentException = System.ArgumentException

local TimeSpan = {}

local function compare(t1, t2)
    if t1.ticks > t2.ticks then return 1 end
    if t1.ticks < t2.ticks then return -1 end
    return 0
end

TimeSpan.compare = compare
TimeSpan.compareTo = compare

function TimeSpan.compareToObj(this, t)
   if t == null then return 1 end
   if getmetatable(t) ~= TimeSpan then
       throw(ArgumentException("Arg_MustBeTimeSpan"))
   end
   compare(this, t)
end

function TimeSpan.equals(t1, t2)
    return t1.ticks == t2.ticks
end

function TimeSpan.equalsObj(this, t)
    if getmetatable(t) == TimeSpan then
        return this.ticks == t.ticks
    end
    return false
end

function TimeSpan.getHashCode(this)
    return this.ticks
end

function TimeSpan.__ctor__(this, ...)
    local ticks
    local length = select("#", ...)
    if length == 1 then
        ticks = ...
    elseif length == 3 then
        local hours, minutes, seconds = ...
        ticks = (((hours * 60 + minutes) * 60) + seconds) * 1e7
    elseif length == 4 then
        local days, hours, minutes, seconds = ...
        ticks = ((((days * 24 + hours) * 60 + minutes) * 60) + seconds) * 1e7
    elseif length == 5 then
        local days, hours, minutes, seconds, milliseconds = ...
        ticks = (((((days * 24 + hours) * 60 + minutes) * 60) + seconds) * 1e3 + milliseconds) * 1e4
    end
    assert(ticks)
    this.ticks = ticks
end

function TimeSpan.getTicks(this) 
    return this.ticks
end

function TimeSpan.getDays(this) 
    return div(this.ticks, 864e9)
end

function TimeSpan.getHours(this) 
    return div(this.ticks, 36e9) % 24
end

function TimeSpan.getMinutes(this) 
   return div(this.ticks, 6e8) % 60
end

function TimeSpan.getSeconds(this) 
    return div(this.ticks, 1e7) % 60
end

function TimeSpan.getMilliseconds(this) 
    return div(this.ticks, 1e4) % 1000
end

function TimeSpan.getTotalDays(this) 
    return this.ticks / 864e9
end

function TimeSpan.getTotalHours(this) 
    return this.ticks / 36e9
end

function TimeSpan.getTotalMilliseconds(this) 
    return this.ticks / 1e4
end

function TimeSpan.getTotalMinutes(this) 
    return this.ticks / 6e8
end

function TimeSpan.getTotalSeconds(this) 
    return this.ticks / 1e7
end

function TimeSpan.add(this, ts) 
    return TimeSpan(this.ticks + ts.ticks)
end

function TimeSpan.subtract(this, ts) 
    return TimeSpan(this.ticks - ts.ticks)
end

function TimeSpan.duration(this) 
    return TimeSpan(math.abs(this.ticks))
end

function TimeSpan.negate(this) 
    return TimeSpan(-this.ticks)
end

function TimeSpan.toString(this) 
    local day = this:getDays()
    local daysStr = day == 0 and "" or (tostring(day) .. ".")
    return string.format("%s%02d:%02d:%02d.%03d", daysStr, this:getHours(), this:getMinutes(), this:getSeconds(), this:getMilliseconds())
end

TimeSpan.__add = TimeSpan.add
TimeSpan.__sub = TimeSpan.subtract

function TimeSpan.__eq(t1, t2)
    return t1.ticks == t2.ticks
end

function TimeSpan.__lt(t1, t2)
    return t1.ticks < t2.ticks
end

function TimeSpan.__le(t1, t2)
    return t1.ticks <= t2.ticks
end

TimeSpan.__tostring = TimeSpan.toString

local new = System.new
local zero = new(TimeSpan, 0)
TimeSpan.zero = zero
TimeSpan.maxValue = new(TimeSpan, 864e13)
TimeSpan.minValue = new(TimeSpan, -864e13)

function TimeSpan.fromDays(value) 
    return TimeSpan(value * 864e9)
end

function TimeSpan.fromHours(value) 
    return TimeSpan(value * 36e9)
end

function TimeSpan.fromMilliseconds(value) 
    return TimeSpan(value * 1e4)
end

function TimeSpan.fromMinutes(value) 
    return TimeSpan(value * 6e8)
end

function TimeSpan.fromSeconds(value) 
    return TimeSpan(value * 1e7)
end

function TimeSpan.fromTicks(value) 
    return TimeSpan(value)
end

function TimeSpan.__defaultVal__()
    return zero
end

System.defStc("System.TimeSpan", TimeSpan)
TimeSpan.__inherits__ = { System.IComparable, System.IComparable_1(TimeSpan), System.IEquatable_1(TimeSpan) }