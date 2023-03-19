using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using LoginRegister.DTOs;
using LoginRegister.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LoginRegister.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _dbContext;

        public UserController(IConfiguration config, AppDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        #region Old DES

        [HttpPost("login")]
        public IActionResult Login(LoginModel model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user exists in database
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                return Unauthorized();
            }

            // Decrypt password using DES encryption
            var passwordBytes = Convert.FromBase64String(user.Password);
            var des = new DESCryptoServiceProvider
            {
                Key = Encoding.UTF8.GetBytes(_config["DESKey"]),
                IV = Encoding.UTF8.GetBytes(_config["DESIV"])
            };
            var password =
                Encoding.UTF8.GetString(des.CreateDecryptor()
                    .TransformFinalBlock(passwordBytes, 0, passwordBytes.Length));

            // Check if password is correct
            if (password != model.Password)
            {
                return Unauthorized();
            }

            // TODO: generate and return authentication token

            return Ok();
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterModel model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if username already exists
            if (_dbContext.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Username already exists");
                return BadRequest(ModelState);
            }

            // Encrypt password using DES encryption
            var passwordBytes = Encoding.UTF8.GetBytes(model.Password);
            var des = new DESCryptoServiceProvider
            {
                Key = Encoding.UTF8.GetBytes(_config["DESKey"]),
                IV = Encoding.UTF8.GetBytes(_config["DESIV"])
            };
            var encryptedPassword =
                Convert.ToBase64String(
                    des.CreateEncryptor().TransformFinalBlock(passwordBytes, 0, passwordBytes.Length));

            // Create user record
            var user = new User
            {
                Username = model.Username,
                Password = encryptedPassword
            };
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            return Ok();
        }

        #endregion
        

        [HttpPost("registerUser")]
        public async Task RegisterUser(string username, string password)
        {
            // Generate a random salt
            var saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            var salt = Convert.ToBase64String(saltBytes);

            // Hash the password using SHA256 with salt
            var hashedPasswordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password + salt));
            var hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            // Store the salt, hashed password, and algorithm name in the User table
            var user = new User
            {
                Username = username,
                Password = hashedPassword,
                PasswordSalt = salt,
                PasswordAlgorithm = "SHA256"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        [HttpPost("changePassword")]
        public async Task ChangePassword(int userId, string newPassword)
        {
            // Generate a new random salt
            var saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            var salt = Convert.ToBase64String(saltBytes);

            // Hash the password using SHA256 with salt
            var hashedPasswordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(newPassword + salt));
            var hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            // Store the old and new passwords in the PasswordMigrations table
            var migration = new PasswordMigration
            {
                UserId = userId,
                OldPassword = _dbContext.Users.Where(u => u.Id == userId).Select(u => u.Password).SingleOrDefault(),
                NewPassword = hashedPassword,
                IsMigrated = false
            };
            _dbContext.PasswordMigrations.Add(migration);

            // Update the User table with the new password
            var user = await _dbContext.Users.FindAsync(userId);
            user.Password = hashedPassword;
            user.PasswordSalt = salt;
            user.PasswordAlgorithm = "SHA256";

            await _dbContext.SaveChangesAsync();
        }

        [HttpPost("Authenticate")]
        public async Task<User> Authenticate(string username, string password)
        {
            // Find the user with the given username
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                // User not found
                return null;
            }

            if (user.PasswordAlgorithm == "SHA256")
            {
                // Check the password using SHA256 with salt
                var hashedPasswordBytes =
                    SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password + user.PasswordSalt));
                var hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

                if (hashedPassword == user.Password)
                {
                    // Password is correct
                    return user;
                }
                else
                {
                    // Password is incorrect
                    return null;
                }
            }
            else if (user.PasswordAlgorithm == "DES")
            {
                // Check the password using DES decryption
                var key = Encoding.UTF8.GetBytes("MySecretKey"); // Replace with your own secret key
                var iv = Encoding.UTF8.GetBytes("MySecretIV"); // Replace with your own secret IV
                var encryptedPasswordBytes = Convert.FromBase64String(user.Password);
                var decryptedPasswordBytes = DecryptDES(encryptedPasswordBytes, key, iv);
                var decryptedPassword = Encoding.UTF8.GetString(decryptedPasswordBytes);

                if (password == decryptedPassword)
                {
                    // Password is correct
                    // Migrate the password to SHA256 with salt
                    await MigratePassword(user);
                    return user;
                }
                else
                {
                    // Password is incorrect
                    return null;
                }
            }
            else
            {
                // Unknown password algorithm
                throw new InvalidOperationException($"Unknown password algorithm: {user.PasswordAlgorithm}");
            }
        }

        private async Task MigratePassword(User user)
        {
            // Generate a new random salt
            var saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            var salt = Convert.ToBase64String(saltBytes);

            // Hash the password using SHA256 with salt
            var hashedPasswordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.Password + salt));
            var hashedPassword = Convert.ToBase64String(hashedPasswordBytes);

            // Store the old and new passwords in the PasswordMigrations table
            var migration = new PasswordMigration
            {
                UserId = user.Id,
                OldPassword = user.Password,
                NewPassword = hashedPassword,
                IsMigrated = false
            };
            _dbContext.PasswordMigrations.Add(migration);

            // Update the User table with the new password
            user.Password = hashedPassword;
            user.PasswordSalt = salt;
            user.PasswordAlgorithm = "SHA256";

            await _dbContext.SaveChangesAsync();
        }

        private static byte[] DecryptDES(byte[] encryptedData, byte[] key, byte[] iv)
        {
            using (var des = DES.Create())
            using (var decryptor = des.CreateDecryptor(key, iv))
            using (var ms = new MemoryStream(encryptedData))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var ms2 = new MemoryStream())
            {
                cs.CopyTo(ms2);
                return ms2.ToArray();
            }
        }
    }
}