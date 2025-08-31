using Blazored.LocalStorage;

namespace HeadStart.Client.Services;

public class LocalStorageService(ILocalStorageService localStorageService) : IStorageService
{
    /// <summary>
    /// Retrieves an item from local storage asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, containing the retrieved item.</returns>
    public ValueTask<T?> GetItemAsync<T>(string key)
    {
        return localStorageService.GetItemAsync<T>(key);
    }

    /// <summary>
    /// Removes an item from local storage asynchronously.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask RemoveItemAsync(string key)
    {
        return localStorageService.RemoveItemAsync(key);
    }

    /// <summary>
    /// Sets an item in local storage asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the item to set.</typeparam>
    /// <param name="key">The key of the item to set.</param>
    /// <param name="value">The value of the item to set.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask SetItemAsync<T>(string key, T value)
    {
        return localStorageService.SetItemAsync(key, value);
    }
}
