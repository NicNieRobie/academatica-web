using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartMath.Api.Users.DBOs
{
    struct LevelExpMapping
    {
        public static readonly SortedDictionary<ulong, string> LevelExpCaps = new SortedDictionary<ulong, string>()
        {
            { 300, "newcomer" },
            { 1500, "apprentice" },
            { 3300, "fast learner" },
            { 5000, "enthusiast" },
            { 7000, "matter expert" },
            { 10000, "virtuose" },
            { 15000, "calculator" }
        };
    }

    public class GetUserResponseDbo
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicURL { get; set; }
        public ulong Exp { get; set; }

        public string Level
        {
            get
            {
                ulong userLevelExpCap = 0;

                foreach (var cap in LevelExpMapping.LevelExpCaps.Keys)
                {
                    if (Exp < cap)
                    {
                        userLevelExpCap = cap;
                        break;
                    }
                }

                return LevelExpMapping.LevelExpCaps[userLevelExpCap];
            }
        }
        public ulong ExpLevelCap
        {
            get
            {
                ulong userLevelExpCap = 0;

                foreach (var cap in LevelExpMapping.LevelExpCaps.Keys)
                {
                    if (Exp < cap)
                    {
                        userLevelExpCap = cap;
                        break;
                    }
                }

                return userLevelExpCap;
            }
        }

        public ulong DaysStreak { get; set; }
        public uint BuoysLeft { get; set; }
    }
}
