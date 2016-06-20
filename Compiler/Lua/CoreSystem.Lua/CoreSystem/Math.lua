local System = System

local Math = math

Math.bigMul = function(a, b) 
    return a * b 
end

Math.divRem = function(a, b) 
    local remainder = a % b;
    return (a - remainder) / b, remainder
end

Math.round = function(n, d, rounding) 
    local m = Math.pow(10, d or 0);
    n = n * m
    local sign = n > 0 and 1 or -1
    if n % 1 == 0.5 * sign then 
        local f = Math.floor(n)
        return (f + (rounding == 1 and (sign > 0) or (f % 2 * sign))) / m
    end
    return Math.floor(n + 0.5) / m
end

Math.sign = function(v) 
    return v == 0 and 0 or (v > 0 and 1 or -1) 
end

Math.trunc = System.trunc
System.Math = Math






