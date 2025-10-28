import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {MatToolbar, MatToolbarModule,} from '@angular/material/toolbar';
import {MatButton, MatButtonModule, MatIconButton} from '@angular/material/button';
import {MatFormField, MatFormFieldModule, MatLabel, MatError, MatHint} from '@angular/material/form-field';
import {MatInput, MatInputModule} from '@angular/material/input';
import {MatTable, MatTableModule} from '@angular/material/table';
import {MatIcon, MatIconModule} from '@angular/material/icon';
import {MatProgressSpinner, MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatCard, MatCardModule, MatCardHeader, MatCardTitle, MatCardContent} from '@angular/material/card';
import {MatCheckbox, MatCheckboxModule} from '@angular/material/checkbox';
import {MatSelect, MatSelectModule} from '@angular/material/select';
import {MatOption} from '@angular/material/core';
import {MatTooltip, MatTooltipModule} from '@angular/material/tooltip';


@NgModule({
  declarations: [],
  imports: [
    MatToolbarModule,
    CommonModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatCheckboxModule,
    MatSelectModule,
    MatTooltipModule
  ],
  exports: [
    MatToolbar,
    MatButton,
    MatFormField,
    MatLabel,
    MatError,
    MatHint,
    MatInput,
    MatTable,
    MatIconButton,
    MatIcon,
    MatProgressSpinner,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardContent,
    MatCheckbox,
    MatSelect,
    MatOption,
    MatTooltip
  ]
})
export class MaterialModule { }
