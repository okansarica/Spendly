namespace Spendly.Shared.Core;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

public class PasswordHelper
{
	//TODO store in a safe place
	private string password = "okan";
	
	public static string HashPassword(string password)
	{
		// Generate a random salt
		byte[] salt = RandomNumberGenerator.GetBytes(16);
        
		// Hash the password with PBKDF2
		string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 600000, // OWASP 2023 recommendation
			numBytesRequested: 32));
        
		// Combine salt and hash for storage
		return $"{Convert.ToBase64String(salt)}.{hashed}";
	}
    
	public static bool VerifyPassword(string password, string? storedHash)
	{
		if (string.IsNullOrEmpty(storedHash))
		{
			throw new Exception("Customer password hash is empty");
		}
		var parts = storedHash.Split('.');
		if (parts.Length != 2)
		{
			return false;
		}
        
		var salt = Convert.FromBase64String(parts[0]);
		var hash = parts[1];
        
		string testHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: password,
			salt: salt,
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 600000,
			numBytesRequested: 32));
            
		return CryptographicOperations.FixedTimeEquals(
			Convert.FromBase64String(hash), 
			Convert.FromBase64String(testHash));
	}
}
