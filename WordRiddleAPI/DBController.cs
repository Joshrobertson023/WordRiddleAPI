using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Collections.ObjectModel;
using WordRiddleAPI.Controllers;
using WordRiddleAPI;
using WordRiddleShared;


/// <summary>
/// This class manages the database connection and operations
/// </summary>


public class DBController
{
    private static string ConnectionString = @"User Id=ADMIN;Password=Josher152003;Data Source=(DESCRIPTION=(RETRY_COUNT=20)(RETRY_DELAY=3)(ADDRESS=(PROTOCOL=TCPS)(HOST=adb.us-ashburn-1.oraclecloud.com)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=gd119454c26aea7_joshwordledb_high.adb.oraclecloud.com))(SECURITY=(SSL_SERVER_DN_MATCH=YES)));";
    //private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ORACLE_CONN_STRING");

    public List<string> usernames = new List<string>(); // List of all usernames

    public string username;    // The current player's username
    public string timeElapsed; // The current player's time in game
    public int guesses;        // The current player's number of guesses
    public int hints;          // The current player's number of hints used
    public int won;            // The current player won a game
    public int wonTimed;       // Player won a timed game
    public int theme;          // The current player's theme
    public string timeTimed;   // Player's time in timed mode
    public int guessesTimed;   // Player's guesses in timed mode
    public int scoreNormal;    // Player's score for normal mode
    public int scoreTimed;     // Player's score for timed mode
    public int viewedInstructions; // Player viewed instructions (0 = no, 1 = normal, 2 = timed)

    /// <summary>
    /// Constructor
    /// </summary>
    public DBController()
    {
    }

    /// <summary>
    /// Grab all the usernames
    /// </summary>
    public void grabUsernames()
    {
        usernames.Clear();
        DataTable dt = new DataTable();
        string query = @"SELECT username FROM WORDLELEADERBOARD";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
        foreach (DataRow row in dt.Rows)
            usernames.Add(row.Field<string>("username"));
    }

    /// <summary>
    /// Add a user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="hints"></param>
    /// <param name="time"></param>
    /// <param name="guesses"></param>
    /// <param name="rankPercentage"></param>
    /// <param name="won"></param>
    public void addUser(string username, int hints = 0, string time = "00:00", 
        int guesses = 0, int won = 0, int theme = 0, int score = 0, int viewedInstructions = 0)
    {
        username = username.Trim().ToLower();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"    
                    BEGIN
                      INSERT INTO WORDLELEADERBOARD (username, hints, time, guesses, won, theme, score, viewedInstructions)
                      VALUES (:username, :hints, :time, :guesses, :won, :theme, :score, :viewedInstructions);
                      INSERT INTO TIMEDLEADERBOARD (username, time, guesses, won, score)
                      VALUES (:username, :time, :guesses, :won, :score);
                    END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.Parameters.Add(new OracleParameter("hints", hints));
        cmd.Parameters.Add(new OracleParameter("time", time));
        cmd.Parameters.Add(new OracleParameter("guesses", guesses));
        cmd.Parameters.Add(new OracleParameter("won", won));
        cmd.Parameters.Add(new OracleParameter("theme", theme));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.Parameters.Add(new OracleParameter("viewedInstructions", viewedInstructions));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Get the info of a user
    /// </summary>
    /// <param name="user"></param>
    public UserInfoDto grabUserInfoPublic(string user)
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT time, username, guesses, hints, won, theme, viewedInstructions 
                     FROM WORDLELEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", user));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);

        if (dt.Rows.Count == 0)
            throw new Exception("User not found");

        string name = dt.Rows[0].Field<string>("username");
        string timeElapsed = dt.Rows[0].Field<string>("time");
        int guesses = Convert.ToInt32(dt.Rows[0]["guesses"]);
        int hints = Convert.ToInt32(dt.Rows[0]["hints"]);
        int won = Convert.ToInt32(dt.Rows[0]["won"]);
        int theme = Convert.ToInt32(dt.Rows[0]["theme"]);
        int viewedInstructions = Convert.ToInt32(dt.Rows[0]["viewedInstructions"]);
        int score = grabNormalScoreForUser(user); // Make this version return just the score

        return new UserInfoDto
        {
            username = name,
            timeElapsed = timeElapsed,
            guesses = guesses,
            hints = hints,
            won = won,
            theme = theme,
            viewedInstructions = viewedInstructions,
            score = score
        };
    }


    /// <summary>
    /// Get the user's score for normal mode
    /// </summary>
    public int grabNormalScoreForUser(string username)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = "SELECT score FROM WORDLELEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        object result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public TimedUserInfoDto grabTimedUserInfoPublic(string username)
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT time, guesses, won FROM TIMEDLEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);

        if (dt.Rows.Count == 0)
            throw new Exception("Timed data not found");

        return new TimedUserInfoDto
        {
            timeTimed = dt.Rows[0].Field<string>("time"),
            guesses = Convert.ToInt32(dt.Rows[0]["guesses"]),
            won = Convert.ToInt32(dt.Rows[0]["won"])
        };
    }


    public TimedScoreDto grabTimedScorePublic(string username)
    {
        DataTable dt = new DataTable();
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"SELECT score FROM TIMEDLEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        using OracleDataAdapter da = new OracleDataAdapter(cmd);
        da.Fill(dt);

        if (dt.Rows.Count == 0)
            throw new Exception("Timed score not found");

        return new TimedScoreDto
        {
            scoreTimed = Convert.ToInt32(dt.Rows[0]["score"])
        };
    }

    public void updateUserInfoPublic(string username, string timeElapsed, int guesses, int score, int hints)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD 
                     SET time = :timeElapsed, guesses = :guesses, score = :score, hints = :hints 
                     WHERE username = :username";

        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("timeElapsed", timeElapsed));
        cmd.Parameters.Add(new OracleParameter("guesses", guesses));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.Parameters.Add(new OracleParameter("hints", hints)); // ✅ new parameter
        cmd.Parameters.Add(new OracleParameter("username", username));

        cmd.ExecuteNonQuery();
    }



    public void updateViewedInstructionsPublic(string username, int viewedInstructions)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET viewedInstructions = :viewedInstructions 
                     WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("viewedInstructions", viewedInstructions));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }


    public void updateUserInfoTimedPublic(string username, string time, int guesses, int score)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE TIMEDLEADERBOARD SET time = :time, guesses = :guesses, score = :score 
                     WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("time", time));
        cmd.Parameters.Add(new OracleParameter("guesses", guesses));
        cmd.Parameters.Add(new OracleParameter("score", score));
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }


    public void editUsernamePublic(string oldUsername, string newUsername)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"BEGIN
                        UPDATE WORDLELEADERBOARD SET username = :newUsername WHERE username = :username;
                        UPDATE TIMEDLEADERBOARD SET username = :newUsername WHERE username = :username;
                     END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("newUsername", newUsername));
        cmd.Parameters.Add(new OracleParameter("username", oldUsername));
        cmd.ExecuteNonQuery();
    }


    public int toggleThemePublic(string username)
    {
        int newTheme;

        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();

        // Toggle theme
        string query = @"
        UPDATE WORDLELEADERBOARD 
        SET theme = CASE WHEN theme = 1 THEN 0 ELSE 1 END 
        WHERE username = :username";

        using (OracleCommand cmd = new OracleCommand(query, conn))
        {
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("username", username));
            cmd.ExecuteNonQuery();
        }

        // Get the new theme value
        using (OracleCommand cmd = new OracleCommand("SELECT theme FROM WORDLELEADERBOARD WHERE username = :username", conn))
        {
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("username", username));
            newTheme = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return newTheme;
    }

    public int GetThemePublic(string username)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();

        string query = "SELECT theme FROM WORDLELEADERBOARD WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn)
        {
            BindByName = true
        };
        cmd.Parameters.Add(new OracleParameter("username", username));

        object result = cmd.ExecuteScalar();
        if (result == null)
            throw new Exception("User not found or theme not set");

        return Convert.ToInt32(result);
    }



    public void setWinPublic(string username)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET won = 1 WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }


    public void setWinTimedPublic(string username)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE TIMEDLEADERBOARD SET won = 1 WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.ExecuteNonQuery();
    }


    public void setHintsPublic(string username, int hints)
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"UPDATE WORDLELEADERBOARD SET hints = :hints WHERE username = :username";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.Parameters.Add(new OracleParameter("hints", hints));
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Grab the leaderboard for normal mode
    /// </summary>
    /// <param name="dt"></param>
    public List<LeaderboardEntry> grabLeaderboard()
    {
        var entries = new List<LeaderboardEntry>();
        string query = @"SELECT RANK() OVER (ORDER BY score) AS rank,
                        username, time, guesses, hints, score
                        FROM WORDLELEADERBOARD WHERE won = 1
                        AND username NOT LIKE 'josher152003'";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleCommand cmd = new OracleCommand(query, conn);
        conn.Open();
        using OracleDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            entries.Add(new LeaderboardEntry
            {
                Rank = Convert.ToInt32(reader["rank"]),
                Username = reader["username"].ToString(),
                Time = reader["time"].ToString(),
                Guesses = Convert.ToInt32(reader["guesses"]),
                Hints = Convert.ToInt32(reader["hints"]),
                Score = Convert.ToInt32(reader["score"])
            });
        }

        return entries;
    }

    //AND username NOT LIKE 'josher152003'

    public List<LeaderboardEntry> grabTimedLeaderboard()
    {
        var entries = new List<LeaderboardEntry>();
        string query = @"SELECT RANK() OVER (ORDER BY score) AS rank,
                     username, time, guesses, score
                     FROM TIMEDLEADERBOARD WHERE won = 1";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleCommand cmd = new OracleCommand(query, conn);
        conn.Open();
        using OracleDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            entries.Add(new LeaderboardEntry
            {
                Rank = Convert.ToInt32(reader["rank"]),
                Username = reader["username"].ToString(),
                Time = reader["time"].ToString(),
                Guesses = Convert.ToInt32(reader["guesses"]),
                Score = Convert.ToInt32(reader["score"])
            });
        }

        return entries;
    }

    public void resetDeveloper()
    {
        using OracleConnection conn = new OracleConnection(ConnectionString);
        conn.Open();
        string query = @"BEGIN
                        DELETE FROM WORDLELEADERBOARD WHERE username = 'josher152003';
                        DELETE FROM TIMEDLEADERBOARD WHERE username = 'josher152003';
                     END;";
        using OracleCommand cmd = new OracleCommand(query, conn);
        cmd.BindByName = true;
        cmd.ExecuteNonQuery();
    }


    public void grabAllUsers(DataTable dt)
    {
        string query = @"SELECT username, time, guesses, hints FROM WORDLELEADERBOARD";
        using OracleConnection conn = new OracleConnection(ConnectionString);
        using OracleDataAdapter da = new OracleDataAdapter(query, conn);
        da.Fill(dt);
    }
}
