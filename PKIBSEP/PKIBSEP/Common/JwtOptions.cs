namespace PKIBSEP.Common
{
    public class JwtOptions
    {
        public static DateTime DefaultExpiresAt => DateTime.UtcNow.AddHours(24);

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string RecaptchaSecret {  get; set; } = string.Empty;
    }
}
