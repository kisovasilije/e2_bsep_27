using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Interfaces;

public interface ICaRepository
{
    Task<List<Certificate2>> GetAllCAsAsync ();

    Task<Certificate2?> GetCaByIdAsync (int id);
}
