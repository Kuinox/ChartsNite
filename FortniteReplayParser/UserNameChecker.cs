namespace FortniteReplayParser
{
    static class UserNameChecker
    {
        /// <summary>
        /// Return true if the username is valid.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool CheckUserName(string userName)
        {
            if (userName.Length < 3 || userName.Length > 16)
            {
                return false;//Tested against Epic Games Account creation. Probably bad because UTF8 username will probably have their length doubled.
            }

            return true;//TODO : better check.
        }
    }
}
