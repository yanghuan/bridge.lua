namespace Test.BridgeIssues.N649
{
    class TestClassA
    {
        public void DoSomething(int i)
        {
            //It should not change case of Console.WriteLine
            Bridge.Html5.Console.WriteLine("Say something");
        }
    }
}
