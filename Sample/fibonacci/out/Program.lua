local System = System

System.namespace("Test", function(namespace)
    namespace.class("Program", function ()
        local main, fibonacciN, fibonacciSequence
        main = function (args) 
            local __t__
            System.Console.writeLine("Print Fibonacci sequence")
            System.Console.writeLine("Input n:")
            local num = System.Console.readLine()
            local count
            __t__, count = System.Int.tryParse(num, count, -2147483648, 2147483647)
            if not __t__ then
                System.throw(System.ArgumentException(System.strconcat(num) .. " is unlawful"))
            end
            local sequence = fibonacciSequence(count)
            for _, v in System.each(sequence) do
                System.Console.write(v)
                System.Console.write(string.char(44))
            end
        end
        fibonacciN = function (n) 
            if n == 0 then
                return 0
            elseif n == 1 then
                return 1
            else
                return fibonacciN(n - 1) + fibonacciN(n - 2)
            end
        end
        fibonacciSequence = function (n) 
            return System.yieldEnumerator(function ()
                do
                    local i = 0
                    while i < n do
                        System.yieldReturn(fibonacciN(i))
                        i = i + 1
                    end
                end
            end)
        end
        return {
            main = main
        }
    end)
end)