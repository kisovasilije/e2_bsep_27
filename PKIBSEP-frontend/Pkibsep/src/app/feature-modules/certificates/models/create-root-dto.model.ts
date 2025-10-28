import { X500NameDto } from './x500-name.model';
import { KeyUsageDto } from './key-usage.model';

export interface CreateRootDto {
  targetCaUserId?: number;
  subject: X500NameDto;
  validityDays: number;
  pathLenConstraint?: number;
  keyUsage: KeyUsageDto;
}
