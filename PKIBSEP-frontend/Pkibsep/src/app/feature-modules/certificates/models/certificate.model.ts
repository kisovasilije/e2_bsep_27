export interface CertificateDto {
  id: number;
  serialHex: string;
  subjectDN: string;
  issuerDN: string;
  notBeforeUtc: Date;
  notAfterUtc: Date;
  isCa: boolean;
  pathLenConstraint?: number;
  pemCert: string;
  chainPem: string;
}
