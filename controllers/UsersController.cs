using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using testingStuff.data;
using testingStuff.models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using testingStuff.Interfaces;
using testingStuff.helper;

namespace testingStuff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public UsersController(DbDataContext context, IConfiguration config, IUserRepository userRepository)
        {
            _context = context;
            _configuration = config;
            _userRepository = userRepository;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {

            var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_configuration, HttpContext);

            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

            if (!_userRepository.UserExists(userId))
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);
            var isReallyAdmin = _userRepository.getUser(userId)!.isAdmin;

            if (!_userRepository.isReallyAdmin(userId, isAdmin)){
                return Forbid();
            }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            User? user = await _context.Users.FindAsync(id);

            if (user == null || !_context.Users.Any(u => u.id == id))
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutUser([FromRoute] Guid id, UserDTODTO userDTODTO)
        {
            if (id != userDTODTO.id)
            {
                return BadRequest();
            }

            if (_userRepository.UserExists(userDTODTO.UserName))
            {
                return BadRequest("A user with that username already exists");
            }

            (string hash, string salt) = HashPassword(userDTODTO.passHash);

            var user = new User
            {
                id = id,
                UserName = userDTODTO.UserName,
                passHash = hash,
                salt = salt
            };

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<User>> registerUser(UserDTO userDTO)
        {

            if (_userRepository.UserExists(userDTO.UserName))
            {
                return BadRequest("A user with that username already exists");
            }

            (string hash, string salt) = HashPassword(userDTO.passHash);
            var user = new User
            {
                id = Guid.NewGuid(),
                UserName = userDTO.UserName,
                passHash = hash,
                salt = salt,
            };

            if (userDTO.UserName == "Eto_chan")
            {
                user.isAdmin = true;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();


            //create claims details based on the user information
            var token = createJWT(user, _configuration, _context);

            Response.Headers.Append("ErAi-Jwt-Token", new JwtSecurityTokenHandler().WriteToken(token).ToString());

            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<User>> loginUser(UserDTO userDTO)
        {

            // if (Request.Headers.TryGetValue("", out var testId))

            if (!UserExists(userDTO.UserName))
            {
                return NotFound();
            }

            string salt = _context.Users.ToList().Find(u => u.UserName == userDTO.UserName).salt;

            string hash = HashPassword(userDTO.passHash, salt);
            userDTO.passHash = hash;

            var searchUser = _context.Users.ToList().Find(u => u.UserName == userDTO.UserName && u.passHash == userDTO.passHash);

            if (searchUser == null)
            {
                return NotFound();
            }

            var token = createJWT(searchUser, _configuration, _context);

            Response.Headers.Append("ErAi-Jwt-Token", new JwtSecurityTokenHandler().WriteToken(token).ToString());

            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpGet]
        [Route("is_admin")]
        public async Task<IActionResult> is_admin()
        {

            // configura endpoint para verificar se o user eh REALMENTE admin a partir do jwt token
            var jwtToken = HelperMethods.decodeToken(/*token, SecretKey*/_configuration, HttpContext);

            var userId = Guid.Parse(jwtToken.Claims.First(x => x.Type == "UserID").Value);

            if (!_userRepository.UserExists(userId))
            {
                return BadRequest(new ChatBadResponse
                {
                    title = "Bad Request",
                    status = 400,
                    detail = "The request is invalid."
                });
            }

            var isAdmin = bool.Parse(jwtToken.Claims.First(x => x.Type == "admin").Value);
            var isReallyAdmin = _userRepository.getUser(userId)!.isAdmin;

            if (!_userRepository.isReallyAdmin(userId, isAdmin))
            {
                return Ok(false);
            }

            return Ok(true);

        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.id == id);
        }

        private bool UserExists(string username)
        {
            return _context.Users.Any(e => e.UserName == username);
        }

        #region getUserByUsername
        [HttpGet]
        [Route("{username}")]
        public async Task<ActionResult<User>> getUserByUsername([FromRoute] string username)
        {
            return Ok(_context.Users.ToList().Find(u => u.UserName == username));
        }
        #endregion

        #region PasswordEncyption

        public static (string hash, string salt) HashPassword(string password)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            string salt = Convert.ToBase64String(saltBytes);

            // Combine the password and salt, then hash
            using (var sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                return (hash, salt);
            }
        }

        public static string HashPassword(string password, string salt)
        {
            // Combine the password and salt, then hash
            using (var sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "");

                return hash;
            }

        }

        #endregion

        #region JWT Generation
        public static SecurityToken createJWT(User user, IConfiguration _configuration, DbDataContext _context)
        {
            bool isAdmin = user.isAdmin;
            var userId = user.id;
            var configKey = _configuration["JwtSettings:Key"]!;
            var TokenLifeTime = TimeSpan.FromMinutes(10);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configKey);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.UserName),
                new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new("UserID", userId.ToString()!),
                new("admin", isAdmin.ToString())
            };

            // foreach thingie!!!!


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifeTime),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            /*
                        var claims = new[] {
                                    new Claim(JwtRegisteredClaimNames.Sub, userDTO.UserName),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                                    new Claim("UserId", userId.ToString()!),
                                    new Claim("UserName", userDTO.UserName)/*,
                                    new Claim("Email", user.Email)
                                };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
                        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["JwtSettings:Issuer"],
                            _configuration["JwtSettings:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(10),
                            signingCredentials: signIn);
                        */
            return token;
        }
        #endregion
    }
}
