using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using WordRiddleShared;


namespace WordRiddleAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DBController _dbController;

        public UserController(DBController dbController)
        {
            _dbController = dbController;
        }

        [HttpGet("usernames")]
        public IActionResult GetUsernames()
        {
            _dbController.grabUsernames();
            return Ok(_dbController.usernames); // Assuming usernames is a public List<string>
        }


        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] UserDto user)
        {
            try
            {
                _dbController.addUser(
                    user.username,
                    user.hints,
                    user.time,
                    user.guesses,
                    user.won,
                    user.theme,
                    user.score,
                    user.viewedInstructions
                );

                return Ok(new { message = "User added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to add user", error = ex.Message });
            }
        }

        [HttpGet("info/{username}")]
        public IActionResult GetUserInfo(string username)
        {
            try
            {
                var info = _dbController.grabUserInfoPublic(username);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch user info", error = ex.Message });
            }
        }

        [HttpGet("timed-info/{username}")]
        public IActionResult GetTimedUserInfo(string username)
        {
            try
            {
                var info = _dbController.grabTimedUserInfoPublic(username);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch timed user info", error = ex.Message });
            }
        }

        [HttpGet("timed-score/{username}")]
        public IActionResult GetTimedScore(string username)
        {
            try
            {
                var score = _dbController.grabTimedScorePublic(username);
                return Ok(score);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch timed score", error = ex.Message });
            }
        }


        [HttpPut("update-info")]
        public IActionResult UpdateUserInfo([FromBody] UpdateUserInfoDto userInfo)
        {
            try
            {
                _dbController.updateUserInfoPublic(userInfo.username, userInfo.timeElapsed, userInfo.guesses, userInfo.score);
                return Ok(new { message = "User info updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update user info", error = ex.Message });
            }
        }


        [HttpPut("update-viewed-instructions")]
        public IActionResult UpdateViewedInstructions([FromBody] UpdateViewedInstructionsDto data)
        {
            try
            {
                _dbController.updateViewedInstructionsPublic(data.username, data.viewedInstructions);
                return Ok(new { message = "Viewed instructions updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update viewed instructions", error = ex.Message });
            }
        }


        [HttpPut("update-timed-info")]
        public IActionResult UpdateTimedUserInfo([FromBody] UpdateTimedUserInfoDto data)
        {
            try
            {
                _dbController.updateUserInfoTimedPublic(data.username, data.time, data.guesses, data.score);
                return Ok(new { message = "Timed user info updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update timed user info", error = ex.Message });
            }
        }


        [HttpPut("edit-username")]
        public IActionResult EditUsername([FromBody] EditUsernameDto data)
        {
            try
            {
                _dbController.editUsernamePublic(data.oldUsername, data.newUsername);
                return Ok(new { message = "Username updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update username", error = ex.Message });
            }
        }


        [HttpPut("toggle-theme/{username}")]
        public IActionResult ToggleTheme(string username)
        {
            try
            {
                int newTheme = _dbController.toggleThemePublic(username);
                return Ok(new { theme = newTheme });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to toggle theme", error = ex.Message });
            }
        }


        [HttpPut("set-win/{username}")]
        public IActionResult SetWin(string username)
        {
            try
            {
                _dbController.setWinPublic(username);
                return Ok(new { message = "Win status set successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to set win status", error = ex.Message });
            }
        }


        [HttpPut("set-win-timed/{username}")]
        public IActionResult SetWinTimed(string username)
        {
            try
            {
                _dbController.setWinTimedPublic(username);
                return Ok(new { message = "Timed win status set successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to set timed win status", error = ex.Message });
            }
        }


        [HttpPut("set-hints")]
        public IActionResult SetHints([FromBody] SetHintsDto dto)
        {
            try
            {
                _dbController.setHintsPublic(dto.username, dto.hints);
                return Ok(new { message = "Hints updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update hints", error = ex.Message });
            }
        }


        [HttpGet("leaderboard")]
        public IActionResult GetLeaderboard()
        {
            try
            {
                var entries = _dbController.grabLeaderboard();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch leaderboard", error = ex.Message });
            }
        }


        [HttpGet("leaderboard-timed")]
        public IActionResult GetTimedLeaderboard()
        {
            try
            {
                var entries = _dbController.grabTimedLeaderboard();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch timed leaderboard", error = ex.Message });
            }
        }


        [HttpDelete("reset-developer")]
        public IActionResult ResetDeveloper()
        {
            try
            {
                _dbController.resetDeveloper();
                return Ok(new { message = "Developer data reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reset developer data", error = ex.Message });
            }
        }


        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            try
            {
                DataTable dt = new DataTable();
                _dbController.grabAllUsers(dt);

                var users = new List<UserSummaryDto>();
                foreach (DataRow row in dt.Rows)
                {
                    users.Add(new UserSummaryDto
                    {
                        Username = row["username"].ToString(),
                        Time = row["time"].ToString(),
                        Guesses = Convert.ToInt32(row["guesses"]),
                        Hints = Convert.ToInt32(row["hints"])
                    });
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch users", error = ex.Message });
            }
        }


        [HttpGet("theme/{username}")]
        public IActionResult GetTheme(string username)
        {
            try
            {
                int theme = _dbController.GetThemePublic(username);
                return Ok(new { theme });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get theme", error = ex.Message });
            }
        }


    }
}
