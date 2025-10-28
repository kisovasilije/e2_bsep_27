using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using PKIBSEP.Dtos;

namespace PKIBSEP.Models.Mappers
{
    /// <summary>
    /// Builds a BouncyCastle X509Name from an API DTO, skipping empty attributes.
    /// </summary>
    public static class X500NameMapper
    {
        public static X509Name ToX509Name(X500NameDto dto)
        {
            var oids = new List<DerObjectIdentifier>();
            var vals = new List<string>();

            void add(string? v, DerObjectIdentifier oid)
            {
                if (!string.IsNullOrWhiteSpace(v)) { oids.Add(oid); vals.Add(v); }
            }

            add(dto.CN, X509Name.CN);
            add(dto.OU, X509Name.OU);
            add(dto.O, X509Name.O);
            add(dto.L, X509Name.L);
            add(dto.ST, X509Name.ST);
            add(dto.C, X509Name.C);

            return new X509Name(oids, vals);
        }
    }
}
