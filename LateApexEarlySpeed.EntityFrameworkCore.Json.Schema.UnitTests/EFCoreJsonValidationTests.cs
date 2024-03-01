using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LateApexEarlySpeed.Json.Schema.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;

#if NETCOREAPP3_1
using LateApexEarlySpeed.EntityFrameworkCore.V3.Json.Schema;
#elif NET6_0
using LateApexEarlySpeed.EntityFrameworkCore.V6.Json.Schema;
#else
Make error happen if target framework names configured incorrectly
#endif

namespace LateApexEarlySpeed.EntityFrameworkCore.Json.Schema.UnitTests
{
    public class EfCoreJsonValidationTests : IDisposable
    {
        private readonly SqliteConnection _sqliteConnection;
        private readonly TestDbContext _dbContext;

        public EfCoreJsonValidationTests()
        {
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();

            DbContextOptions<TestDbContext> dbContextOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_sqliteConnection).Options;
            _dbContext = new TestDbContext(dbContextOptions);
            
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task SaveChangesAsync_ValidJsonData_Success()
        {
            await _dbContext.TestModels.AddAsync(new TestModel
            {
                Id = 1,
                JsonContent = """
                {
                  "A": "abcde",
                  "B": 5,
                  "C": [1, 2, 3, "4", 5],
                  "D": {"Prop": true}
                }
                """
            });

            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task SaveChangesAsync_InvalidArrayItem_Fail()
        {
            await _dbContext.TestModels.AddAsync(new TestModel
            {
                Id = 1,
                JsonContent = """
                {
                  "A": "abcde",
                  "B": 5,
                  "C": [1, 2, 3, null, 5], 
                  "D": {"Prop": true}
                }
                """
            });

            DbUpdateException dbUpdateException = await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
            Assert.NotNull(dbUpdateException.InnerException);
            Assert.IsType<JsonValidationException>(dbUpdateException.InnerException);
            
            JsonValidationException jsonValidationException = (JsonValidationException)dbUpdateException.InnerException;
            Assert.Equal("Failed to validate json property: 'JsonContent'. Failed json location (json pointer format): '/C/3', reason: Expect type(s): 'Object|Array|Boolean|Number|String' but actual is 'Null'.", 
                jsonValidationException.Message);
            Assert.Null(jsonValidationException.InnerException);
            Assert.False(jsonValidationException.DetailedInfo.IsValid);
            Assert.Equal("Expect type(s): 'Object|Array|Boolean|Number|String' but actual is 'Null'", jsonValidationException.DetailedInfo.ErrorMessage);
            Assert.Equal(ImmutableJsonPointer.Create("/C/3"), jsonValidationException.DetailedInfo.InstanceLocation);
        }

        [Fact]
        public async Task SaveChangesAsync_AddAndUpdateAndDeleteAndRecreate()
        {
            string validJson = """
                {
                  "A": "abcde",
                  "B": 5,
                  "C": [1, 2, 3, 4, 5], 
                  "D": 0
                }
                """;

            // Create row
            await _dbContext.TestModels.AddAsync(new TestModel
            {
                Id = 1,
                JsonContent = validJson
            });

            await _dbContext.SaveChangesAsync();

            TestModel testModel = await _dbContext.TestModels.FirstAsync(model => model.Id == 1);
            Assert.Equal(validJson, testModel.JsonContent);

            // Try to update column to invalid json
            string invalidJson = """
                {
                  "A": "abcde",
                  "B": 5,
                  "C": [1, 2, null, 4, 5], 
                  "D": 0
                }
                """;

            testModel.JsonContent = invalidJson;
            DbUpdateException dbUpdateException = await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
            Assert.IsType<JsonValidationException>(dbUpdateException.InnerException);

            DetachAllEntities();

            testModel = await _dbContext.TestModels.FirstAsync(model => model.Id == 1);
            Assert.Equal(validJson, testModel.JsonContent);

            // Update column to valid json
            validJson = """
                {
                  "A": "edcba",
                  "B": 6,
                  "C": [1, 2, 3, 4, 5, 6], 
                  "D": false
                }
                """;

            testModel.JsonContent = validJson;
            await _dbContext.SaveChangesAsync();

            DetachAllEntities();

            testModel = await _dbContext.TestModels.FirstAsync();
            Assert.Equal(validJson, testModel.JsonContent);

            // Delete row
            _dbContext.TestModels.Remove(testModel);
            await _dbContext.SaveChangesAsync();

            Assert.Equal(0, _dbContext.TestModels.Count());

            // Recreate this row
            await _dbContext.TestModels.AddAsync(new TestModel
            {
                Id = 1,
                JsonContent = validJson
            });

            await _dbContext.SaveChangesAsync();

            testModel = await _dbContext.TestModels.FirstAsync();
            Assert.Equal(validJson, testModel.JsonContent);

            // Try to update column to invalid json
            invalidJson = """
                {
                  "A": "abcde",
                  "B": 5,
                  "C": [1, 2, null, 4, 5], 
                  "D": 0
                }
                """;

            testModel.JsonContent = invalidJson;
            dbUpdateException = await Assert.ThrowsAsync<DbUpdateException>(() => _dbContext.SaveChangesAsync());
            Assert.IsType<JsonValidationException>(dbUpdateException.InnerException);
        }

        /// <summary>
        /// For target net3.1, there is no ChangeTracker.Clear() method, so create this method for entities tracking clear.
        /// </summary>
        private void DetachAllEntities()
        {
            List<EntityEntry> undetachedEntriesCopy = _dbContext.ChangeTracker.Entries().Where(e => e.State != EntityState.Detached).ToList();

            foreach (var entry in undetachedEntriesCopy)
            {
                entry.State = EntityState.Detached;
            }
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();

            _sqliteConnection.Dispose();
        }
    }
}