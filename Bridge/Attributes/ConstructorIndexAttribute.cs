using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge {
    [External]
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class ConstructorIndexAttribute : Attribute {
        public ConstructorIndexAttribute(int value) {
        }
    }
}
