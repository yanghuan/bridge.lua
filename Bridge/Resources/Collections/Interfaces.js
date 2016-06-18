// @source /Collections/Interfaces.js

Bridge.define('Bridge.IEnumerable');
Bridge.define('Bridge.IEnumerator');
Bridge.define('Bridge.IEqualityComparer');
Bridge.define('Bridge.ICollection', {
    inherits: [Bridge.IEnumerable]
});

Bridge.Class.generic('Bridge.IEnumerator$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.IEnumerator$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.IEnumerator]
    }));
});

Bridge.Class.generic('Bridge.IEnumerable$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.IEnumerable$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.IEnumerable]
    }));
});

Bridge.Class.generic('Bridge.ICollection$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.ICollection$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.IEnumerable$1(T)]
    }));
});

Bridge.Class.generic('Bridge.IEqualityComparer$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.IEqualityComparer$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
    }));
});

Bridge.Class.generic('Bridge.IDictionary$2', function (TKey, TValue) {
    var $$name = Bridge.Class.genericName('Bridge.IDictionary$2', TKey, TValue);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.IEnumerable$1(Bridge.KeyValuePair$2(TKey, TValue))]
    }));
});

Bridge.Class.generic('Bridge.IList$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.IList$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.ICollection$1(T)]
    }));
});

Bridge.Class.generic('Bridge.IComparer$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.IComparer$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
    }));
});

Bridge.Class.generic('Bridge.ISet$1', function (T) {
    var $$name = Bridge.Class.genericName('Bridge.ISet$1', T);

    return Bridge.Class.cache[$$name] || (Bridge.Class.cache[$$name] = Bridge.define($$name, {
        inherits: [Bridge.ICollection$1(T)]
    }));
});