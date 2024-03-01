using System.Text.Json;
using Microsoft.EntityFrameworkCore;

#if NETCOREAPP3_1
using LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema;
#elif NET6_0
using LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema;
#else
Make error happen if target framework names configured incorrectly
#endif

namespace LateApexEarlySpeed.EntityFrameworkCore.Json.Schema.UnitTests;

public class TestDbContext : DbContext
{
    public DbSet<TestModel> TestModels { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestModel>().Property(b => b.JsonContent).HasJsonValidation(b =>
        {
            b.IsJsonObject()
                .HasProperty("A", b => b.IsJsonString().HasMinLength(5))
                .HasProperty("B", b => b.IsJsonNumber().IsGreaterThan(1).IsLessThan(10))
                .HasProperty("C", b => b.IsJsonArray().HasMinLength(5).HasItems(b =>
                {
                    b.NotJsonNull();
                }))
                .HasProperty("D", b => b.Or(
                        b => b.IsJsonFalse(),
                        b => b.IsJsonNumber().Equal(0),
                        b => b.IsJsonObject().HasCustomValidation(element => element.GetProperty("Prop").ValueKind == JsonValueKind.True,
                            jsonElement => $"Cannot pass my custom validation, data is {jsonElement}")
                    )
                );
        });
    }
}