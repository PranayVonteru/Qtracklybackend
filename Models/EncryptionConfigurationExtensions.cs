
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Demoproject.Extensions
{
    public static class EncryptionConfigurationExtensions
    {
        public static void EnsureEncryptionKeysExist(this IConfiguration configuration)
        {
            var aesKey = configuration["Encryption:AESKey"];
            var aesIV = configuration["Encryption:AESIV"];

            if (string.IsNullOrEmpty(aesKey) || string.IsNullOrEmpty(aesIV))
            {
                UpdateAppSettingsWithEncryptionKeys();
            }
        }

        private static void UpdateAppSettingsWithEncryptionKeys(string appSettingsPath = "appsettings.json")
        {
            try
            {
                // Generate new encryption keys
                var (key, iv) = GenerateAesKeyAndIV();

                // Read existing appsettings.json
                string jsonContent = File.ReadAllText(appSettingsPath);

                // Parse JSON into JsonDocument for better handling
                using var document = JsonDocument.Parse(jsonContent);
                var rootElement = document.RootElement;

                // Convert to dictionary for modification
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
                    WriteIndented = true,
                    PropertyNamingPolicy = null // Keep original property names
                };

                string updatedJson = JsonSerializer.Serialize(appSettings, options);
                File.WriteAllText(appSettingsPath, updatedJson);

                Console.WriteLine("Encryption keys have been successfully updated in appsettings.json");
                Console.WriteLine($"Generated AES Key: {key}");
                Console.WriteLine($"Generated AES IV: {iv}");
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
}