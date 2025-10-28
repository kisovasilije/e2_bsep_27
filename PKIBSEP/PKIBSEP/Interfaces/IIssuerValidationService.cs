namespace PKIBSEP.Interfaces
{
    public interface IIssuerValidationService
    {
        Task ValidateAsync(int issuerId, bool issuingCa, int? requestedPathLen);
    }
}
