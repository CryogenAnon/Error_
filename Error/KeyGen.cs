using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class KeyGen
{
    static void Main()
    {
        // Generate AES key
        byte[] aesKey = GenerateAESKey();

        // Display the generated key in hexadecimal format
        Console.WriteLine("Generated AES Key: " + BitConverter.ToString(aesKey).Replace("-", ""));

        // Save the key to a PEM file
        SaveKeyToPemFile(aesKey, "key.pem");

        Console.WriteLine("Key saved to key.pem");
    }

    static byte[] GenerateAESKey()
    {
        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            return aes.Key;
        }
    }

    static void SaveKeyToPemFile(byte[] key, string filePath)
    {
        // Convert the byte array to Base64 encoding
        string base64Key = Convert.ToBase64String(key);

        // Create the PEM content
        string pemContent = $"-----BEGIN AES KEY-----\n{base64Key}\n-----END AES KEY-----";

        // Write the content to the specified file
        File.WriteAllText(filePath, pemContent);
    }
}
