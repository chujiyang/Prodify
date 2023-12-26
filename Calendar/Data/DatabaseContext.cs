using SQLite;
using System.Linq.Expressions;

namespace Calendar.Data;

public class DatabaseContext : IAsyncDisposable
{
    private const string DbName = "Calendar.db3";
    private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, DbName);

    private SQLiteAsyncConnection _connection;
    private SQLiteAsyncConnection Database =>
        (_connection ??= new SQLiteAsyncConnection(DbPath,
            SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache));

    private async Task CreateTableIfNotExistsAsync<TTable>() where TTable : class, new()
    {
        await Database.CreateTableAsync<TTable>();
    }

    public async Task<AsyncTableQuery<TTable>> GetTableAsync<TTable>() where TTable : class, new()
    {
        await CreateTableIfNotExistsAsync<TTable>();
        return Database.Table<TTable>();
    }

    public async Task<IEnumerable<TTable>> GetAllAsync<TTable>() where TTable : class, new()
    {
        var table = await GetTableAsync<TTable>();
        return await table.ToListAsync();
    }

    public async Task<IEnumerable<TTable>> GetFileteredAsync<TTable>(Expression<Func<TTable, bool>> predicate) where TTable : class, new()
    {
        var table = await GetTableAsync<TTable>();
        return await table.Where(predicate).ToListAsync();
    }

    private async Task<TResult> ExecuteAsync<TTable, TResult>(Func<Task<TResult>> action) where TTable : class, new()
    {
        await CreateTableIfNotExistsAsync<TTable>();
        return await action();
    }

    public async Task<TTable> GetItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
    {
        //await CreateTableIfNotExists<TTable>();
        //return await Database.GetAsync<TTable>(primaryKey);
        return await ExecuteAsync<TTable, TTable>(async () => await Database.GetAsync<TTable>(primaryKey));
    }

    public async Task<bool> AddItemAsync<TTable>(TTable item) where TTable : class, new()
    {
        //await CreateTableIfNotExists<TTable>();
        //return await Database.InsertAsync(item) > 0;
        return await ExecuteAsync<TTable, bool>(async () => await Database.InsertAsync(item) > 0);
    }

    public async Task<bool> UpdateItemAsync<TTable>(TTable item) where TTable : class, new()
    {
        await CreateTableIfNotExistsAsync<TTable>();
        return await Database.UpdateAsync(item) > 0;
    }

    public async Task<bool> DeleteItemAsync<TTable>(TTable item) where TTable : class, new()
    {
        await CreateTableIfNotExistsAsync<TTable>();
        return await Database.DeleteAsync(item) > 0;
    }

    public async Task<bool> DeleteItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
    {
        await CreateTableIfNotExistsAsync<TTable>();
        return await Database.DeleteAsync<TTable>(primaryKey) > 0;
    }

    public async ValueTask DisposeAsync() => await _connection?.CloseAsync();
}
