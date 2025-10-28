namespace PKIBSEP.Common.Enum
{
    public enum CertificateType
    {
        RootCa = 0,        // self-signed CA at the top of the chain
        IntermediateCa = 1,// CA signed by another CA, can issue further certs
        EndEntity = 2      // leaf (server/client/device), does not issue further certs
    }
}
