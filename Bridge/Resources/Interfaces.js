    // @source Interfaces.js

    Bridge.define("Bridge.IFormattable", {
        statics: {
            $is: function (obj) {
                if (Bridge.isNumber(obj)) {
                    return true;
                }

                if (Bridge.isDate(obj)) {
                    return true;
                }

                return Bridge.is(obj, Bridge.IFormattable, true);
            }
        }
    });

    Bridge.define("Bridge.IComparable");

    Bridge.define("Bridge.IFormatProvider");

    Bridge.define("Bridge.ICloneable");

    Bridge.Class.generic("Bridge.IComparable$1", function (T) {
        var $$name = Bridge.Class.genericName("Bridge.IComparable$1", T);

        return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name));
    });

    Bridge.Class.generic("Bridge.IEquatable$1", function (T) {
        var $$name = Bridge.Class.genericName("Bridge.IEquatable$1", T);

        return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name));
    });

    Bridge.define("Bridge.IPromise");
    Bridge.define("Bridge.IDisposable");
