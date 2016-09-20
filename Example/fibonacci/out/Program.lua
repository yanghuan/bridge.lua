local System = System

System.namespace("Test", function(namespace)
    namespace.class("Program", function ()
        local Main, FibonacciN, FibonacciSequence
        Main = function (args) 
            local __t__
            System.Console.WriteLine("Print Fibonacci sequence")
            System.Console.WriteLine("Input n:")
            local num = System.Console.ReadLine()
            local count
            __t__, count = System.Int.TryParse(num, count)
            if not __t__ then
                System.throw(System.ArgumentException(System.strconcat(num) .. " is unlawful"))
            end
            local sequence = FibonacciSequence(count)
            for _, v in System.each(sequence) do
                System.Console.Write(v)
                System.Console.WriteChar(44)
            end
        end
        FibonacciN = function (n) 
            if n == 0 then
                return 0
            elseif n == 1 then
                return 1
            else
                return FibonacciN(n - 1) + FibonacciN(n - 2)
            end
        end
        FibonacciSequence = function (n) 
            return System.yieldEnumerator(function ()
                do
                    local i = 0
                    while i < n do
                        System.yieldReturn(FibonacciN(i))
                        i = i + 1
                    end
                end
            end, System.Int)
        end
        return {
            Main = Main
        }
    end)
end)