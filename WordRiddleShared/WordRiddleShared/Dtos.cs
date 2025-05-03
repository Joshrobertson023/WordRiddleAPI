using System.Collections.ObjectModel;

namespace WordRiddleShared
{
    public class VersionEntry
    {
        public string Version { get; set; }
        public string Link { get; set; }
        public string ChangeLog { get; set; }
    }
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public string Time { get; set; }
        public int Guesses { get; set; }
        public int Hints { get; set; }
        public int Score { get; set; }
    }


    public class TimedLeaderboardEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; }
        public string Time { get; set; }
        public int Guesses { get; set; }
        public int Score { get; set; }
    }

    public class TimedUserInfoDto
    {
        public string timeTimed { get; set; }
        public int guesses { get; set; }
        public int won { get; set; }
    }


    // DTO class for incoming JSON data
    public class UserDto
    {
        public string username { get; set; }
        public int hints { get; set; } = 0;
        public string time { get; set; } = "00:00";
        public int guesses { get; set; } = 0;
        public int won { get; set; } = 0;
        public int theme { get; set; } = 0;
        public int score { get; set; } = 0;
        public int viewedInstructions { get; set; } = 0;
    }

    public class UserInfoDto
    {
        public string username { get; set; }
        public string timeElapsed { get; set; }
        public int guesses { get; set; }
        public int hints { get; set; }
        public int won { get; set; }
        public int theme { get; set; }
        public int viewedInstructions { get; set; }
        public int score { get; set; }
    }

    public class TimedScoreDto
    {
        public int scoreTimed { get; set; }
    }


    public class UpdateUserInfoDto
    {
        public string username { get; set; }      // Must be passed in from client
        public string timeElapsed { get; set; }
        public int guesses { get; set; }
        public int score { get; set; }
    }


    public class UpdateViewedInstructionsDto
    {
        public string username { get; set; }
        public int viewedInstructions { get; set; }
    }


    public class UpdateTimedUserInfoDto
    {
        public string username { get; set; }
        public string time { get; set; }
        public int guesses { get; set; }
        public int score { get; set; }
    }


    public class EditUsernameDto
    {
        public string oldUsername { get; set; }
        public string newUsername { get; set; }
    }


    public class SetHintsDto
    {
        public string username { get; set; }
        public int hints { get; set; }
    }


    public class UserSummaryDto
    {
        public string Username { get; set; }
        public string Time { get; set; }
        public int Guesses { get; set; }
        public int Hints { get; set; }
    }
}
