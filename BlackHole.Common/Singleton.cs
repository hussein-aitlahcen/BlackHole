namespace BlackHole.Common
{
    public class Singleton<T>
        where T : class, new()
    {
        public static T Instance { get; } = new T();
    }
}
