import { CertificatePreview } from 'src/app/feature-modules/certificates/models/certificate-preview.model';

export function extractCn(subjectDn: string): string | null {
  const cnMatch = subjectDn.match(/CN=([^,]+)/);
  return cnMatch ? cnMatch[1].trim() : null;
}

export function populateCertificateCns(certificates: CertificatePreview[]): void {
  certificates.forEach(cert => {
    cert.issuedToCn = extractCn(cert.issuedTo) || undefined;
    cert.issuedByCn = extractCn(cert.issuedBy) || undefined;
  });
}
