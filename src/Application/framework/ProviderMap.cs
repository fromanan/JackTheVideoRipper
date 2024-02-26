namespace JackTheVideoRipper.framework;

public static class ProviderMap
{
    public static readonly Dictionary<string, Provider> ProviderDomainToEnum = new()
    {
        // YouTube Domains
        { "youtu.be",               Provider.YouTube },
        { "youtube.com",            Provider.YouTube },
        { "googlevideo.com",        Provider.YouTube },
        { "youtube.com.br",         Provider.YouTube },
        { "youtube.co.nz",          Provider.YouTube },
        { "youtube.de",             Provider.YouTube },
        { "youtube.es",             Provider.YouTube },
        { "youtube.it",             Provider.YouTube },
        { "youtube.nl",             Provider.YouTube },
        { "youtube-nocookie.com",   Provider.YouTube },
        { "youtube.ru",             Provider.YouTube },
        { "ytimg.com",              Provider.YouTube },
    };

    public static Provider LookupFromDomain(string domainUrl)
    {
        if (FileSystem.ParseUrl(domainUrl) is not { } domainInfo)
            return Provider.Default;

        if (ProviderDomainToEnum.TryGetValue(domainInfo.RegistrableDomain, out Provider provider))
            return provider;

        if (ProviderDomainToEnum.TryGetValue(domainInfo.Domain, out provider))
            return provider;

        return Provider.Default;
    }
}

public enum Provider
{
    Default,
    YouTube,
    Vimeo,
    DailyMotion
}