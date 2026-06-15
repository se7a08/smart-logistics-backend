using global::SmartLogistics.Domain.Interfaces;


namespace SmartLogistics.Infrastructure.Services.Auth
{
    
    public class PasswordHasher : IPasswordHasher
    {
        
        private const int SaltWorkFactor = 12;

        public string Hash(string password)
        {
           
            return BCrypt.Net.BCrypt.HashPassword(password, SaltWorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}