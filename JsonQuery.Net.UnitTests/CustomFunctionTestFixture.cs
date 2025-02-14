namespace JsonQuery.Net.UnitTests;

public class CustomFunctionTestFixture
{
    public CustomFunctionTestFixture()
    {
        JsonQueryableRegistry.AddQueryableType<AnyQueryable>("any");
        JsonQueryableRegistry.AddQueryableType<AllQueryable>("all");
    }
}