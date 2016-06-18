// @source /Collections/Comparer.js

Bridge.Class.generic('Bridge.EqualityComparer$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.EqualityComparer$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.IEqualityComparer$1(T)],

        equals: function (x, y) {
            if (!Bridge.isDefined(x, true)) {
                return !Bridge.isDefined(y, true);
            } else if (Bridge.isDefined(y, true)) {
                var isBridge = x && x.$$name;

                if (!isBridge) {
                    return Bridge.equals(x, y);
                }
                else if (Bridge.isFunction(x.equalsT)) {
                    return Bridge.equalsT(x, y);
                }
                else if (Bridge.isFunction(x.equals)) {
                    return Bridge.equals(x, y);
                }

                return x === y;
            }

            return false;
        },

        getHashCode: function (obj) {
            return Bridge.isDefined(obj, true) ? Bridge.getHashCode(obj) : 0;
        }
    }));
});

Bridge.EqualityComparer$1.$default = new Bridge.EqualityComparer$1(Object)();
