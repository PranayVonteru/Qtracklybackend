using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Demoproject.Services.Interfaces;

namespace Demoproject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // Ensure only authenticated users can access
    public class EncryptionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;

        public EncryptionController(IConfiguration configuration, IWebHostEnvironment environment,IUserService userService)
        {
            _configuration = configuration;
            _environment = environment;
            _userService = userService;
        }

        [HttpGet("keys")]

       //// [HttpGet("keys")]
       // public IActionResult GetEncryptionKeys()
       // {
       //     try
       //     {
       //         var (aesKey, aesIV) = _userService.GetEncryptionKeys();

       //         if (string.IsNullOrEmpty(aesKey) || string.IsNullOrEmpty(aesIV))
       //         {
       //             return BadRequest(new { message = "Encryption keys are missing or invalid." });
       //         }

       //         return Ok(new
       //         {
       //             aesKey = aesKey,
       //             aesIV = aesIV,
       //             timestamp = DateTime.UtcNow
       //         });
       //     }
       //     catch (Exception ex)
       //     {
       //         return StatusCode(500, new { message = "Error retrieving encryption keys", error = ex.Message });
       //     }
       // }

        public IActionResult GetEncryptionKeys()
        {
            try
            {
                var aesKey = _configuration["Encryption:AESKey"];
                var aesIV = _configuration["Encryption:AESIV"];

                // Check if keys exist, if not generate them
                if (string.IsNullOrEmpty(aesKey) || string.IsNullOrEmpty(aesIV))
                {
                    // Generate and update keys
                    var (newKey, newIV) = GenerateAndUpdateEncryptionKeys();

                    if (string.IsNullOrEmpty(newKey) || string.IsNullOrEmpty(newIV))
                    {
                        return BadRequest(new { message = "Failed to generate or retrieve encryption keys" });
                    }

                    Environment.SetEnvironmentVariable("AES_SECRET_KEY", newKey);
                    Environment.SetEnvironmentVariable("AES_SECRET_IV", newIV);

                    aesKey = newKey;
                    aesIV = newIV;
                }

                return Ok(new
                {
                    aesKey = aesKey,
                    aesIV = aesIV,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving encryption keys", error = ex.Message });
            }
        }

        [HttpPost("regenerate")]
        public IActionResult RegenerateKeys()
        {
            try
            {
                // Generate new keys and update appsettings
                var (aesKey, aesIV) = GenerateAndUpdateEncryptionKeys();

                if (string.IsNullOrEmpty(aesKey) || string.IsNullOrEmpty(aesIV))
                {
                    return BadRequest(new { message = "Failed to regenerate encryption keys" });
                }

                return Ok(new
                {
                    message = "New encryption keys generated successfully",
                    aesKey = aesKey,
                    aesIV = aesIV,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error regenerating encryption keys", error = ex.Message });
            }
        }

        [HttpGet("public-key")]
        public IActionResult GetPublicKeyForKeyExchange()
        {
            try
            {
                // Generate RSA key pair for secure key exchange
                using (var rsa = RSA.Create(2048))
                {
                    var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                    var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

                    // Store private key in session or cache for later use
                    HttpContext.Session.SetString("RSAPrivateKey", privateKey);

                    return Ok(new
                    {
                        publicKey = publicKey,
                        keyId = Guid.NewGuid().ToString(),
                        expiresAt = DateTime.UtcNow.AddMinutes(5) // Key expires in 5 minutes
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating public key", error = ex.Message });
            }
        }

        [HttpPost("secure-keys")]
        public IActionResult GetSecureEncryptionKeys([FromBody] SecureKeyRequest request)
        {
            try
            {
                // Get private key from session
                var privateKeyBase64 = HttpContext.Session.GetString("RSAPrivateKey");
                if (string.IsNullOrEmpty(privateKeyBase64))
                {
                    return BadRequest(new { message = "No active key exchange session found" });
                }

                using (var rsa = RSA.Create())
                {
                    rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKeyBase64), out _);

                    var aesKey = _configuration["Encryption:AESKey"];
                    var aesIV = _configuration["Encryption:AESIV"];

                    // Encrypt the keys using RSA
                    var encryptedAESKey = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(aesKey), RSAEncryptionPadding.OaepSHA256));
                    var encryptedAESIV = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(aesIV), RSAEncryptionPadding.OaepSHA256));

                    // Clear the private key from session
                    HttpContext.Session.Remove("RSAPrivateKey");

                    return Ok(new
                    {
                        encryptedAESKey = encryptedAESKey,
                        encryptedAESIV = encryptedAESIV,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error providing secure keys", error = ex.Message });
            }
        }

        private (string Key, string IV) GenerateAndUpdateEncryptionKeys()
        {
            try
            {
                // Generate new encryption keys
                var (key, iv) = GenerateAesKeyAndIV();

                // Get the appsettings.json path
                var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");

                // Read existing appsettings.json
                string jsonContent = System.IO.File.ReadAllText(appSettingsPath);

                // Parse JSON into a dictionary
                var appSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

                if (appSettings == null)
                {
                    appSettings = new Dictionary<string, object>();
                }

                // Create or update the Encryption section
                var encryptionSection = new Dictionary<string, string>
                {
                    ["AESKey"] = key,
                    ["AESIV"] = iv
                };

                appSettings["Encryption"] = encryptionSection;

                // Write back to file with proper formatting
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string updatedJson = JsonSerializer.Serialize(appSettings, options);
                System.IO.File.WriteAllText(appSettingsPath, updatedJson);

                // Force configuration reload
                if (_configuration is IConfigurationRoot configRoot)
                {
                    configRoot.Reload();
                }

                Console.WriteLine("Encryption keys have been successfully updated in appsettings.json");
                Console.WriteLine($"Generated AES Key: {key}");
                Console.WriteLine($"Generated AES IV: {iv}");

                return (key, iv);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appsettings.json: {ex.Message}");
                throw;
            }
        }

        private static (string Key, string IV) GenerateAesKeyAndIV()
        {
            byte[] keyBytes = new byte[32]; // 256-bit key
            byte[] ivBytes = new byte[16];  // 128-bit IV

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
                rng.GetBytes(ivBytes);
            }

            return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(ivBytes));
        }
    }

    public class SecureKeyRequest
    {
        public string KeyId { get; set; }
        public string ClientIdentifier { get; set; }
    }
}