export interface Ca {
  id: number;
  subjectDn: string;
  notBefore: Date;
  notAfter: Date;
  cn?: string;
}
