using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace SiteQuadra.Services;

public interface IAdminSecurityService
{
    Task<string> InitializeAdminPasswordAsync();
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateSecureToken();
}

public class AdminSecurityService : IAdminSecurityService
{
    private readonly IConfiguration _configuration;
    private readonly string _passwordFilePath;
    
    public AdminSecurityService(IConfiguration configuration)
    {
        _configuration = configuration;
        _passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "admin_security.dat");
    }
    
    public async Task<string> InitializeAdminPasswordAsync()
    {
        // Verifica se já existe senha configurada
        if (File.Exists(_passwordFilePath))
        {
            var existingHash = await File.ReadAllTextAsync(_passwordFilePath);
            if (!string.IsNullOrWhiteSpace(existingHash))
            {
                return null; // Senha já configurada
            }
        }
        
        // Gera nova senha forte
        var newPassword = GenerateStrongPassword();
        var hashedPassword = HashPassword(newPassword);
        
        // Salva o hash da senha
        await File.WriteAllTextAsync(_passwordFilePath, hashedPassword);
        
        // Log da senha gerada (apenas na primeira vez)
        Console.WriteLine("=====================================");
        Console.WriteLine("🔐 SENHA ADMINISTRATIVA GERADA:");
        Console.WriteLine($"   {newPassword}");
        Console.WriteLine("=====================================");
        Console.WriteLine("⚠️  IMPORTANTE: Salve esta senha!");
        Console.WriteLine("   Ela não será mostrada novamente.");
        Console.WriteLine("=====================================");
        
        return newPassword;
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;
            
        var inputHash = HashPassword(password);
        return inputHash == hashedPassword;
    }
    
    public string GenerateSecureToken()
    {
        // Gera token seguro para sessão (válido por tempo determinado)
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
    
    private string GenerateStrongPassword()
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "0123456789";
        const string symbols = "!@#$%^&*";
        const string allChars = uppercase + lowercase + numbers + symbols;
        
        var password = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            // Garante pelo menos um de cada tipo
            password.Append(GetRandomChar(uppercase, rng));
            password.Append(GetRandomChar(lowercase, rng));
            password.Append(GetRandomChar(numbers, rng));
            password.Append(GetRandomChar(symbols, rng));
            
            // Completa com caracteres aleatórios até 12 caracteres
            for (int i = 4; i < 12; i++)
            {
                password.Append(GetRandomChar(allChars, rng));
            }
        }
        
        // Embaralha a senha
        return new string(password.ToString().OrderBy(x => Guid.NewGuid()).ToArray());
    }
    
    private char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var value = BitConverter.ToUInt32(bytes, 0);
        return chars[(int)(value % chars.Length)];
    }
    
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = password + "SiteQuadra_Salt_2024"; // Salt fixo para consistência
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    
    public async Task<string> GetStoredPasswordHashAsync()
    {
        if (!File.Exists(_passwordFilePath))
            return null;
            
        return await File.ReadAllTextAsync(_passwordFilePath);
    }
}