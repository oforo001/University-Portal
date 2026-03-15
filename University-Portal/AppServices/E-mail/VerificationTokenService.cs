using System.Security.Cryptography;

namespace University_Portal.AppServices.E_mail
{
    public class VerificationTokenService : IVerificationTokenService
    {
        public string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();

            var bytes = new byte[4];
            rng.GetBytes(bytes);

            int number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;

            return number.ToString("D6");
        }
    }
}
