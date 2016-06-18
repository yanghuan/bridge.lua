using Bridge;

namespace TestIssue379
{
    [ObjectLiteral(DefaultValueMode.Ignore)]
    public class DataIgnore
    {
        public DataIgnore()
        {
        }

        public DataIgnore(DefaultValueMode mode)
        {

        }

        public int Int1;
        public int Int2 = 2;
        public string Str3;
        public string Str4 = "Str4";
        public int? IntNull5;
        public int? IntNull6 = 6;
        public decimal Decimal7;
        public decimal Decimal8 = 8;
    }


    [ObjectLiteral(DefaultValueMode.DefaultValue)]
    public class DataDefaultValue
    {
        public DataDefaultValue()
        {
        }

        public DataDefaultValue(DefaultValueMode mode)
        {

        }

        public int Int1;
        public int Int2 = 2;
        public string Str3;
        public string Str4 = "Str4";
        public int? IntNull5;
        public int? IntNull6 = 6;
        public decimal Decimal7;
        public decimal Decimal8 = 8;
    }

    [ObjectLiteral(DefaultValueMode.Initializer)]
    public class DataInitializer
    {
        public DataInitializer()
        {
        }

        public DataInitializer(DefaultValueMode mode)
        {

        }

        public int Int1;
        public int Int2 = 2;
        public string Str3;
        public string Str4 = "Str4";
        public int? IntNull5;
        public int? IntNull6 = 6;
        public decimal Decimal7;
        public decimal Decimal8 = 8;
    }


    public class Tests
    {
        public void TestDataIgnore()
        {
            var d1 = new DataIgnore();
            var d2 = new DataIgnore() { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88};

            var d3 = new DataIgnore(DefaultValueMode.Ignore);
            var d4 = new DataIgnore(DefaultValueMode.Ignore) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d5 = new DataIgnore(DefaultValueMode.DefaultValue);
            var d6 = new DataIgnore(DefaultValueMode.DefaultValue) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d7 = new DataIgnore(DefaultValueMode.Initializer);
            var d8 = new DataIgnore(DefaultValueMode.Initializer) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };
        }

        public void TestDataDefaultValue()
        {
            var d1 = new DataDefaultValue();
            var d2 = new DataDefaultValue() { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d3 = new DataDefaultValue(DefaultValueMode.Ignore);
            var d4 = new DataDefaultValue(DefaultValueMode.Ignore) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d5 = new DataDefaultValue(DefaultValueMode.DefaultValue);
            var d6 = new DataDefaultValue(DefaultValueMode.DefaultValue) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d7 = new DataDefaultValue(DefaultValueMode.Initializer);
            var d8 = new DataDefaultValue(DefaultValueMode.Initializer) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };
        }

        public void TestDataInitializer()
        {
            var d1 = new DataInitializer();
            var d2 = new DataInitializer() { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d3 = new DataInitializer(DefaultValueMode.Ignore);
            var d4 = new DataInitializer(DefaultValueMode.Ignore) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d5 = new DataInitializer(DefaultValueMode.DefaultValue);
            var d6 = new DataInitializer(DefaultValueMode.DefaultValue) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };

            var d7 = new DataInitializer(DefaultValueMode.Initializer);
            var d8 = new DataInitializer(DefaultValueMode.Initializer) { Int1 = 1, Int2 = 22, Str3 = "3", Str4 = "Str44", IntNull5 = 5, IntNull6 = 66, Decimal7 = 7, Decimal8 = 88 };
        }
    }
}
