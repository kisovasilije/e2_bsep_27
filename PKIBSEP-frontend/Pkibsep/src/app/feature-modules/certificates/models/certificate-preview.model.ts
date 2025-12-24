export interface CertificatePreview {
  readonly id: number;

  /**
   * Subject Distinguished Name of the entity to which the certificate is issued
   */
  issuedTo: string;

  /**
   * Common Name (CN) extracted from the Subject DN of the entity to which the certificate is issued
   */
  issuedToCn?: string;

  /**
   * Subject Distinguished Name of the CA that issued the certificate
   */
  issuedBy: string;

  /**
   * Common Name (CN) extracted from the Subject DN of the CA that issued the certificate
   */
  issuedByCn?: string;

  notBefore: Date;
  notAfter: Date;
  pem: string;
}

export type ReadonlyCertificatePreview = Readonly<CertificatePreview>;
