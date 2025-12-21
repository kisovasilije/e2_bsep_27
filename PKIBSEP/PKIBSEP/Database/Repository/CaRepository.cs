using Microsoft.EntityFrameworkCore;
using PKIBSEP.Interfaces;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Database.Repository;

public class CaRepository : ICaRepository
{
    private readonly ApplicationDbContext context;

    private readonly DbSet<Certificate2> certificates;

    public CaRepository (ApplicationDbContext context)
    {
        this.context = context;
        
        certificates = context.Set<Certificate2>();
    }

    public async Task<List<Certificate2>> GetAllCAsAsync()
    {
        return await certificates
            .Where(c => c.Type == Common.Enum.CertificateType.IntermediateCa)
            .ToListAsync();
    }

    public async Task<Certificate2?> GetCaByIdAsync (int id)
    {
        return await certificates
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
