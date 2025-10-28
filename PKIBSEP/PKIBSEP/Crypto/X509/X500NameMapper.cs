using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;

namespace PKIBSEP.Crypto.X509
{
    /// <summary>
    /// Maps individual X.500 attributes (CN, O, OU, L, ST, C) to a BouncyCastle X509Name.
    /// All fields are optional except CN in most flows; only non-empty attributes are included.
    /// </summary>
    public static class X500NameMapper
    {
        /// <param name="commonName">CN – Common Name (e.g., CA display name or server FQDN)</param>
        /// <param name="organization">O – Organization</param>
        /// <param name="organizationalUnit">OU – Organizational Unit</param>
        /// <param name="locality">L – Locality / City</param>
        /// <param name="stateOrProvince">ST – State / Province</param>
        /// <param name="country">C – Country (ISO 3166-1 alpha-2)</param>
        public static X509Name ToX509Name(
            string? commonName,
            string? organization,
            string? organizationalUnit,
            string? locality,
            string? stateOrProvince,
            string? country)
        {
            var oids = new List<DerObjectIdentifier>();
            var values = new List<string>();

            void AddIfPresent(string? value, DerObjectIdentifier oid)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    oids.Add(oid);
                    values.Add(value);
                }
            }

            AddIfPresent(commonName, X509Name.CN);
            AddIfPresent(organizationalUnit, X509Name.OU);
            AddIfPresent(organization, X509Name.O);
            AddIfPresent(locality, X509Name.L);
            AddIfPresent(stateOrProvince, X509Name.ST);
            AddIfPresent(country, X509Name.C);

            return new X509Name(oids, values);
        }
    }
}
