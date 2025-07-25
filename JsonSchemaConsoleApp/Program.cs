using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonQuery.Net;
using JsonQuery.Net.Queryables;
using LateApexEarlySpeed.Json.Schema;
using LateApexEarlySpeed.Json.Schema.Common;
using LateApexEarlySpeed.Json.Schema.FluentGenerator;
using LateApexEarlySpeed.Json.Schema.Generator;

namespace JsonSchemaConsoleApp
{
    class GenericClass2<T1, T2> where T1 : struct, IEquatable<T1>
    {
        public T1 Property1 { get; set; }
        public T1? Property2 { get; set; }
        public GenericClass<T1, List<T2>> Property3 { get; set; }
        public GenericClass<int, List<T1?>> Property4 { get; set; }
        public GenericClass<int, List<T1?>?> Property5 { get; set; }
        public T2 Property6 { get; set; }
        public T2? Property7 { get; set; }
    }

    class GenericClass<T1, T2>
    {
        public Dictionary<string, List<T1?>> Dic { get; set; }

        public InnerClass<string?> Prop { get; set; }

        public class InnerClass<T>
        {
            public T1 A { get; set; }
        }
    }

    public interface IGenericInterface<T1> : IGenericClass3<List<T1?>>
    {
    }

    class GenericClass3<T1> : IGenericClass3<List<T1?>>
    {
        public Dictionary<string, List<T1?>> Dic { get; set; }

        public GenericClass<int, string>.InnerClass<string?> Prop { get; set; }
    }

    public interface IGenericClass3<T>
    {
        Dictionary<string, T> Dic { get; set; }
        Dictionary<string, T> Function() => throw new NotImplementedException();
    }

    class GenericClass4<T1> : BaseGenericClass<List<T1>> where T1 : notnull
    {
        public override Dictionary<string, List<T1>> Dic { get; set; }
    }

    public class BaseGenericClass<T>
    {
        public virtual Dictionary<string, T> Dic { get; set; }
    }

    public interface ITestDto : IGenericClass3<List<string>?>
    {
    }

    struct MyStruct<T>
    {
        
    }

    class OuterClass
    {
        class TestClass<T> : BaseGenericClass<List<T>>
        {
            public Dictionary<string, string> Property { get; set; }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // IJsonQueryable jsonQueryable = JsonQueryable.Parse("""
            //     {
            //       names: map(.name),
            //       count: size(),
            //       averageAge: map(.age) | average()
            //     }
            //     """);
            //
            // string input = """
            //     [
            //       {"name": "Chris", "age": 23, "city": "New York"},
            //       {"name": "Emily", "age": 19, "city": "Atlanta"},
            //       {"name": "Joe", "age": 32, "city": "New York"},
            //       {"name": "Kevin", "age": 19, "city": "Atlanta"},
            //       {"name": "Michelle", "age": 27, "city": "Los Angeles"},
            //       {"name": "Robert", "age": 45, "city": "Manhattan"},
            //       {"name": "Sarah", "age": 31, "city": "New York"}
            //     ]
            //     
            //     """;
            // string result1 = jsonQueryable.Query(JsonNode.Parse(input))!.ToJsonString();
            //
            // return;
            //
            // Type type = typeof(IGenericClass3<string>);
            //
            // MethodInfo property = type.GetMethod("Function")!;
            // MethodInfo property2 = type.GetMethod("Function")!;
            //
            // // Dic<string, List<string>?>
            // // NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(property);
            //
            // Type propertyType = typeof(GenericClass3<int>).GetProperty("Prop").PropertyType;
            //
            // // Dictionary<string, List<StringBuilder?>> dictionary = new GenericClass4<StringBuilder>().Dic;
            // // List<StringBuilder?> stringBuilders = dictionary.Values.First();
            //
            // List<StringBuilder?> first = new GenericClass<StringBuilder, int>().Dic.Values.First();
            //
            // // new TestClass<string>().Property = null;
            //
            // PropertyInfo propertyInfo = typeof(TestClass).GetProperty("Property");
            // NullabilityState nullabilityState = new NullabilityInfoContext().Create(propertyInfo).ReadState;
            //
            //
            // JsonSchemaBuilder builder = new JsonSchemaBuilder();
            //
            // builder.IsJsonString().Equal("abc").IsIn(new string[] { "abc", "def" }).HasMaxLength(10).HasMinLength(3).HasPattern(".*a").HasCustomValidation(s => s.StartsWith('a'), s => s);
            // builder.IsJsonNumber().Equal(3d).Equal(1).Equal(1UL).IsIn(new double[] { 1, 2, 3 }).IsGreaterThan(1).IsLessThan(10).NotGreaterThan(11).NotLessThan(0).MultipleOf(1.5)
            //     .HasCustomValidation((double d) => Math.Abs(d - 1.5) < 0.001, d => "").HasCustomValidation((long ll) => true, ll => "");
            // // builder.IsJsonObject().Equivalent("""{"A": "a", "B": 1.0001}""").SerializationEquivalent(new { A = "a", B = 1.0001 }).HasNoProperty("aaa").HasProperty("A").HasProperty("A", b => b.IsJsonString()).HasProperty("B")
            // //     .HasCustomValidation<TestDto>(tc => true, tc => "").HasCustomValidation(typeof(TestDto), tc => true, tc => "").HasCustomValidation(element => true, element => "");
            // // builder.IsJsonArray().Equivalent("[]").NotContains(b => b.IsJsonString()).Contains(b => b.IsJsonString()).SerializationEquivalent(new object[] { }).HasItems(b => b.IsJsonString()).HasLength(8).HasMaxLength(10).HasMinLength(1).HasUniqueItems().HasCustomValidation<TestDto>(array => true, array => "").HasCustomValidation(element => true, element => "");
            // builder.IsJsonNull();
            // builder.NotJsonNull();
            // builder.IsJsonBoolean();
            // builder.IsJsonTrue();
            // builder.IsJsonFalse();
            // // builder.IsDateTimeOffset().Equal(DateTimeOffset.UtcNow).Before(DateTimeOffset.UtcNow).After(DateTimeOffset.UtcNow).HasCustomValidation(dt => true, dt => "");
            // // builder.IsDateTime().Equal(DateTime.UtcNow).Before(DateTime.UtcNow).After(DateTime.UtcNow).HasCustomValidation(dt => true, dt => "");
            // // builder.IsGuid();
            // builder.Or(b => b.IsJsonObject(), b => b.IsJsonString(), b => b.IsJsonNull());
            //
            // JsonValidator validator = builder.BuildValidator();
            // ValidationResult ret = validator.Validate("""{"A": "a", "B": 1.0002}""");

            // JsonValidator validator = JsonSchemaGenerator.GenerateJsonValidator<TestClass>();

            string jsonSchema = File.ReadAllText("schema.json");
            string instance = File.ReadAllText("instance.json");

            var jsonValidator = new JsonValidator(jsonSchema);
            ValidationResult validationResult = jsonValidator.Validate(instance);

            if (validationResult.IsValid)
            {
                Console.WriteLine("good");
            }
            else
            {
                ValidationError validationError = validationResult.ValidationErrors.First();
                Console.WriteLine($"Failed keyword: {validationError.Keyword}");
                Console.WriteLine($"ResultCode: {validationError.ResultCode}");
                Console.WriteLine($"Error message: {validationError.ErrorMessage}");
                Console.WriteLine($"Failed instance location: {validationError.InstanceLocation}");
                Console.WriteLine($"Failed relative keyword location: {validationError.RelativeKeywordLocation}");
                Console.WriteLine($"Failed schema resource base uri: {validationError.SchemaResourceBaseUri}");
            }

            while (true)
            {
                ValidationResult result = jsonValidator.Validate(instance);
            }
        }
    }
}