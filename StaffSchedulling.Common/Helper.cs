using System.Text;

namespace StaffScheduling.Common
{
    public static class Helper
    {
        public static T[] EnumArray<T>() where T : System.Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string AddSpacesToString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return String.Empty;
            }

            StringBuilder newStr = new StringBuilder(str.Length * 2);
            newStr.Append(str[0]);

            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) && str[i - 1] != ' ')
                    newStr.Append(' ');
                newStr.Append(str[i]);
            }

            return newStr.ToString();
        }
    }
}
