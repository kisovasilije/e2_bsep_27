using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Interfaces;

public interface ICaRepository
{
    Task<List<Certificate2>> GetAllCAsAsync ();

    Task<Certificate2?> GetCaByIdAsync (int id);

    Task<Certificate2?> CreateAsync (Certificate2 certificate);

    Task<bool> ExistsAsync (string csrHashHex, int caId);

    Task<bool> ExistsAsync (int id);

    Task<List<Certificate2>> GetAllWithIssuerByUserId (int userId);

    Task<Certificate2?> GetByIdAsync (int id);

    Task SaveChangesAsync();

    Task<List<Certificate2>> GetIntermediateCaCerttificatesAsync();

    Task<Certificate2?> GetByIssuerIdAndSerialNumber(int issuerId, string serialNumberHex);
}
