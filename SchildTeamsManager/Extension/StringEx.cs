namespace SchildTeamsManager.Extension
{
    public static class StringEx
    {
        public static string WithoutStartingZero(this string input)
        {
            return input.TrimStart('0');
        }
    }
}
