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

namespace testingStuff.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DbDataContext _context;

        public UsersController(DbDataContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> PutUser([FromRoute]Guid id, User user)
        {
            if (id != user.id)
            {
                return BadRequest();
            }

            (string hash, string salt) = HashPassword(user.passHash);
            user.passHash = hash;
            user.salt = salt;

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
        public async Task<ActionResult<User>> registerUser(User user)
        {
            user.id = Guid.NewGuid();
            (string hash, string salt) = HashPassword(user.passHash);
            user.passHash = hash;
            user.salt = salt;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.id }, user);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<User>> loginUser(User user)
        {
            (string hash, string salt) = HashPassword(user.passHash);
            user.passHash = hash;
            user.salt = salt;
            
            var searchUser = _context.Users.ToList().Find(u => u.UserName == user.UserName && u.passHash == user.passHash);
            
            if (searchUser == null){
                return NotFound();
            }

            return Ok(searchUser);
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

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.id == id);
        }

        #region getUserByUsername
        [HttpGet]
        [Route("{username}")]
        public async Task<ActionResult<User>> getUserByUsername([FromRoute] string username){
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

        #endregion
    }
}
