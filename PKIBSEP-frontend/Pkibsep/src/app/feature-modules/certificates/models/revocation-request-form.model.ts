import { FormControl } from '@angular/forms';

export interface RevocationRequestForm {
  reason: FormControl<number | null>;
  comment: FormControl<string | null>;
}
