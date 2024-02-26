using JackTheVideoRipper.models;
using JackTheVideoRipper.models.containers;
using JackTheVideoRipper.viewmodels;

namespace JackTheVideoRipper.framework;

internal static class AppUpdate
{
    public static readonly Notification UpToDateNotification = new(Messages.UpToDate, typeof(AppUpdate));
        
    public static async Task CheckForNewAppVersion(bool isStartup = true)
    {
        // if sender obj is bool then version being checked on startup passively and dont show dialog that it's up to date
        AppVersionModel result = await AppVersionModel.GetFromServer();
        switch (result is {IsNewerVersionAvailable: true})
        {
            case true when Modals.Update(result):
                FileSystem.GetWebResourceHandle(Urls.ApplicationUpdate);
                break;
            case false when isStartup:
                NotificationsManager.SendNotification(UpToDateNotification);
                break;
        }
    }
}