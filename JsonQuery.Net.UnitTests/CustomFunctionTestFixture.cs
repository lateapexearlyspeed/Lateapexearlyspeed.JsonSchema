namespace JsonQuery.Net.UnitTests;

public class CustomFunctionTestFixture
{
    public CustomFunctionTestFixture()
    {
        JsonQueryableRegistry.AddQueryableType<AnyTestQueryable>("anyTest");
        JsonQueryableRegistry.AddQueryableType<AllTestQueryable>("allTest");
    }
}