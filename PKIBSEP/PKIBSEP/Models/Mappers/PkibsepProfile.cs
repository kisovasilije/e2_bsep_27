using AutoMapper;
using PKIBSEP.Dtos;
using PKIBSEP.Dtos.Session;
using PKIBSEP.Models.Certificate;

namespace PKIBSEP.Models.Mappers;

public class PkibsepProfile : Profile
{
    public PkibsepProfile()
    {
        CreateMap<Session, SessionDto>();
        CreateMap<Certificate.Certificate, CertificateDto>()
            .ForMember(d => d.SubjectDN, m => m.MapFrom(s => s.SubjectDistinguishedName))
            .ForMember(d => d.IssuerDN, m => m.MapFrom(s => s.IssuerDistinguishedName))
            .ForMember(d => d.IsCa, m => m.MapFrom(s => s.IsCertificateAuthority));
    }
}
