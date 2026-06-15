using System;

namespace SmartLogistics.Domain.Interfaces
{
    
    public interface IJwtService
    {
       
        string GenerateAccessToken(Guid userId, string email, string role);

        
        Guid? ValidateToken(string token);
    }
}