﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Encryptor
{
    private byte[] aesKey;

    public Encryptor(string keyFilePath)
    {
        // Load AES key from the PEM file
        aesKey = LoadKeyFromPemFile(keyFilePath);
    }

    public byte[] EncryptData(byte[] data)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = aesKey;

            // Use a random IV (Initialization Vector) for each encryption
            aes.GenerateIV();

            // Encrypt the data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                }

                // Combine IV and encrypted data
                byte[] ivAndEncryptedData = new byte[aes.IV.Length + memoryStream.ToArray().Length];
                Array.Copy(aes.IV, ivAndEncryptedData, aes.IV.Length);
                Array.Copy(memoryStream.ToArray(), 0, ivAndEncryptedData, aes.IV.Length, memoryStream.ToArray().Length);

                return ivAndEncryptedData;
            }
        }
    }

    private byte[] LoadKeyFromPemFile(string filePath)
    {
        // Read the content of the PEM file
        string pemContent = File.ReadAllText(filePath);

        // Extract the Base64-encoded key
        string base64Key = pemContent
            .Replace("-----BEGIN AES KEY-----", "")
            .Replace("-----END AES KEY-----", "")
            .Replace("\n", "");

        // Convert Base64 to byte array
        return Convert.FromBase64String(base64Key);
    }
}

class Program
{
    static void Main()
    {
        // Directory path to scan for files
        string directoryPath = "D:\\Test";

        // File paths
        string keyFilePath = "key.pem";

        // Load the AES key from the PEM file
        Encryptor encryptor = new Encryptor(keyFilePath);

        // Scan the directory for files
        string[] files = Directory.GetFiles(directoryPath);

        foreach (string filePath in files)
        {
            // Skip the key file itself
            if (filePath.ToLower() == keyFilePath.ToLower())
                continue;

            // Read the data from the input file
            byte[] inputData = File.ReadAllBytes(filePath);

            // Encrypt the data
            byte[] encryptedData = encryptor.EncryptData(inputData);

            // Save the encrypted data to a new file with the same name and "_encrypted" suffix
            string outputFile = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_encrypted" + Path.GetExtension(filePath));
            File.WriteAllBytes(outputFile, encryptedData);

            // Delete the original file
            File.Delete(filePath);

            Console.WriteLine($"Encryption completed. Original file deleted. Encrypted data saved to {outputFile}");
        }
    }
}