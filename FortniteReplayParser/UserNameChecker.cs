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
            if (userName.Length < 3 || userName.Length > 16) return false;
            return true;//TODO : better check.
        }
    }
}
