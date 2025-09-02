using System.Globalization;
using System.Security.Cryptography;
using Blazored.LocalStorage;

namespace HeadStart.Client.Services.Notifications;

public class InMemoryNotificationService(
    ILocalStorageService localStorageService) : INotificationService
{
    private const string LocalStorageKey = "__notficationTimestamp";

    private readonly List<NotificationMessage> _messages = new();

    public async Task<bool> AreNewNotificationsAvailable()
    {
        var timestamp = await GetLastReadTimestamp().ConfigureAwait(false);
        var entriesFound = _messages.Any(x => x.PublishDate > timestamp);

        return entriesFound;
    }

    public async Task MarkNotificationsAsRead()
    {
        await localStorageService.SetItemAsync(LocalStorageKey, DateTime.UtcNow.Date).ConfigureAwait(false);
    }

    public async Task MarkNotificationsAsRead(string id)
    {
        var message = await GetMessageById(id).ConfigureAwait(false);

        var timestamp = await localStorageService.GetItemAsync<DateTime?>(LocalStorageKey).ConfigureAwait(false);
        if (timestamp.HasValue)
        {
            await localStorageService.SetItemAsync(LocalStorageKey, message.PublishDate).ConfigureAwait(false);
        }
    }

    public Task<NotificationMessage> GetMessageById(string id)
    {
        return Task.FromResult(_messages.First(x => x.Id.Equals(id)));
    }

    public async Task<IDictionary<NotificationMessage, bool>> GetNotifications()
    {
        var lastReadTimestamp = await GetLastReadTimestamp().ConfigureAwait(false);
        var items = _messages.ToDictionary(x => x, x => lastReadTimestamp > x.PublishDate);
        return items;
    }

    public Task AddNotification(NotificationMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    private async Task<DateTime> GetLastReadTimestamp()
    {
        try
        {
            if ((await localStorageService.GetItemAsync<DateTime?>(LocalStorageKey)) is null)
            {
                return DateTime.MinValue;
            }

            var timestamp = await localStorageService.GetItemAsync<DateTime>(LocalStorageKey).ConfigureAwait(false);
            return timestamp;
        }
        catch (CryptographicException)
        {
            await localStorageService.RemoveItemAsync(LocalStorageKey).ConfigureAwait(false);
            return DateTime.MinValue;
        }
    }

    public void Preload()
    {
        _messages.Add(new NotificationMessage(
            "readme",
            "Blazor Application is ready",
            "We are paving the way for the future of Blazor",
            "Announcement",
            DateTime.UtcNow,
            "img/avatar.png",
            [
                new NotificationAuthor("Hualin",
                    "https://avatars.githubusercontent.com/u/1549611?s=48&v=4")
            ], typeof(NotificationMessage)));
    }
}
