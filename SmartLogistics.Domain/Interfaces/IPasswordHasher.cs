namespace SmartLogistics.Domain.Interfaces
{
    // واجهة لتشفير كلمات السر عشان منحفظهاش في الداتا بيز كنص واضح
    public interface IPasswordHasher
    {
        // دالة بتحول الباسورد لـ Hash
        string Hash(string password);

        // دالة بتتأكد إن الباسورد اللي المستخدم دخله مطابق للـ Hash اللي عندنا
        bool Verify(string password, string hash);
    }
}