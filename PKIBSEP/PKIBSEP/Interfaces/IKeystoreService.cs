using PKIBSEP.Models.Certificate;
using System.Security.Cryptography.X509Certificates;

namespace PKIBSEP.Interfaces
{
    public interface IKeystoreService
    {
        Task SaveCaPfxAsync(
            int caUserId,
            int certificateId,
            X509Certificate2 certificateWithPrivateKey,
            X509Certificate2Collection? issuerChain = null);

        Task<(X509Certificate2 IssuerCertificate, string PfxPassword)> LoadIssuerAsync(int caUserId, CaKeyMaterial caKeyMaterial);
    }
}
