namespace Test
{
#if (TEST1)
    public class ExistsIfTest1Defined { }
#endif
#if (TEST2)
    public class ExistsIfTest2Defined { }
#endif
#if (TEST3)
    public class NotExistsTest3{ }
#endif

}
