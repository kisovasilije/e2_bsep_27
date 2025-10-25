namespace PKIBSEP.Dtos
{
    public class PasswordStrengthDto
    {
        public int Score { get; set; } // 0-4 (very weak to very strong)
        public string Strength { get; set; } = string.Empty; // "Very Weak", "Weak", "Medium", "Strong", "Very Strong"
        public List<string> Suggestions { get; set; } = new();
        public bool MeetsMinimumRequirements { get; set; }
    }
}
