export interface Csr {
  caId: number;
  notBefore: Date;
  notAfter: Date;
  csrPem: string;
}
