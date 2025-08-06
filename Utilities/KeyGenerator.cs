//using System;
//using System.Security.Cryptography;

//namespace Demoproject.Utilities
//{
//    public class KeyGenerator
//    {
//        byte[] keyBytes = new byte[32];
//        byte[] ivBytes = new byte[16];
//        using (var rng = new RNGCryptoServiceProvider())
//        {
//            rng.GetBytes(keyBytes);
//            rng.GetBytes(ivBytes);
//        }
//        return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(ivBytes));
//    }
//}
//using System;
//using System.Security.Cryptography;

//public class KeyGenerator
//{
//    public static (string Key, string IV) GenerateAesKeyAndIV()
//    {
//        byte[] keyBytes = new byte[32]; // 256-bit key
//        byte[] ivBytes = new byte[16];  // 128-bit IV
//        using (var rng = new RNGCryptoServiceProvider())
//        {
//            rng.GetBytes(keyBytes);
//            rng.GetBytes(ivBytes);
//        }
//        return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(ivBytes));
//    }

//    public static void Main()
//    {
//        var (key, iv) = GenerateAesKeyAndIV();
//        Console.WriteLine("Generated AES Key: oooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo" + key);
//        Console.WriteLine("Generated AES IV: " + iv);
//    }
//}


using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;

namespace Demoproject.Utilities
{
    public class KeyGenerator
    {
        public static (string Key, string IV) GenerateAesKeyAndIV()
        {
            byte[] keyBytes = new byte[32]; // 256-bit key
            byte[] ivBytes = new byte[16];  // 128-bit IV
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(keyBytes);
                rng.GetBytes(ivBytes);
            }
            return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(ivBytes));
        }

        public static void UpdateAppSettingsWithEncryptionKeys(string appSettingsPath = "appsettings.json")
        {
            try
            {
                // Generate new encryption keys
                var (key, iv) = GenerateAesKeyAndIV();

                // Read existing appsettings.json
                string jsonContent = File.ReadAllText(appSettingsPath);

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
                File.WriteAllText(appSettingsPath, updatedJson);

                Console.WriteLine("Encryption keys have been successfully updated in appsettings.json");
                Console.WriteLine($"Generated AES Key: {key}");
                Console.WriteLine($"Generated AES IV: {iv}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appsettings.json: {ex.Message}");
            }
        }

        public static void Main(string[] args)
        {
            // You can specify a custom path if needed
            string appSettingsPath = args.Length > 0 ? args[0] : "appsettings.json";

            Console.WriteLine("Generating encryption keys and updating appsettings.json...");
            UpdateAppSettingsWithEncryptionKeys(appSettingsPath);
        }
    }
}