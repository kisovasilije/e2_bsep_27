import { X500NameDto } from './x500-name.model';

export interface CertificateDto {
  id: number;
  serialNumber: string;
  subject: X500NameDto;
  issuer: X500NameDto;
  validFrom: Date;
  validTo: Date;
  isCA: boolean;
  pathLenConstraint?: number;
}
