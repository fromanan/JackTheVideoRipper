namespace JackTheVideoRipper.extensions;

public static class ObjectExtensions
{
    public static T As<T>(this object obj)
    {
        if (!obj.Is<T>())
            throw new InvalidCastException();

        return (T)obj;
    }

    public static bool Is<T>(this object obj)
    {
        Type objType = obj.GetType();
        return objType.IsAssignableTo(typeof(T)) ||
               objType.IsEquivalentTo(typeof(T)) || 
               objType.IsInstanceOfType(typeof(T));
    }

    public static T Cast<T>(this object obj)
    {
        return (T)obj;
    }
}