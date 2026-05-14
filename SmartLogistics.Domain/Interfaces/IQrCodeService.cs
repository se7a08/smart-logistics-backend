namespace SmartLogistics.Domain.Interfaces
{
    // واجهة لخدمة الـ QR Code عشان نكود الشحنات ونسهل عملية الـ Tracking
    public interface IQrCodeService
    {
        // دالة بتولد نص مشفر للـ QR بناءً على الـ ID بتاع الشحنة
        string GenerateQrCode(Guid shipmentId);

        // دالة بتتأكد إن الـ QR اللي اتعمل له Scan يخص الشحنة دي فعلاً
        bool ValidateQrCode(string qrCode, Guid shipmentId);

        // دالة بتحول النص لـ "صورة" QR عشان نقدر نطبعها أو نعرضها في الموبايل
        byte[] GenerateQrCodeImage(string data);
    }
}