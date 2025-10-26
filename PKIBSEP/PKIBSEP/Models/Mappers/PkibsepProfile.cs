using AutoMapper;
using PKIBSEP.Dtos.Session;

namespace PKIBSEP.Models.Mappers;

public class PkibsepProfile : Profile
{
    public PkibsepProfile()
    {
        CreateMap<Session, SessionDto>();
    }
}
