namespace SmartLogistics.Domain.Interfaces
{
    
    public interface IQrCodeService
    {
        
        string GenerateQrCode(Guid shipmentId);

        
        bool ValidateQrCode(string qrCode, Guid shipmentId);

        byte[] GenerateQrCodeImage(string data);
    }
}