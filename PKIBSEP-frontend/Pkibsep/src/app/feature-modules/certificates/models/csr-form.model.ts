import { FormControl } from '@angular/forms';
import { Ca } from './ca.model';

export interface CsrForm {
  issuer: FormControl<Ca | null>;
  notBefore: FormControl<Date | null>;
  notAfter: FormControl<Date | null>;
  csrPem: FormControl<string | null>;
}
