namespace StaffScheduling.Common
{
    public static class Helper
    {
        public static T[] EnumArray<T>() where T : System.Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
