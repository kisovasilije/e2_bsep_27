import { X500NameDto } from './x500-name.model';
import { KeyUsageDto } from './key-usage.model';

export interface IssueIntermediateDto {
  targetCaUserId?: number;
  issuerId: number;
  subject: X500NameDto;
  validityDays: number;
  pathLenConstraint?: number;
  keyUsage: KeyUsageDto;
}
