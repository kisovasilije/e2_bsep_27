import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import {MatToolbar, MatToolbarModule,} from '@angular/material/toolbar';
import {MatButton, MatButtonModule, MatIconButton} from '@angular/material/button';
import {MatFormField, MatFormFieldModule, MatLabel, MatError} from '@angular/material/form-field';
import {MatInput, MatInputModule} from '@angular/material/input';
import {MatTable, MatTableModule} from '@angular/material/table';
import {MatIcon, MatIconModule} from '@angular/material/icon';
import {MatProgressSpinner, MatProgressSpinnerModule} from '@angular/material/progress-spinner';


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
    MatProgressSpinnerModule
  ],
  exports: [
    MatToolbar,
    MatButton,
    MatFormField,
    MatLabel,
    MatError,
    MatInput,
    MatTable,
    MatIconButton,
    MatIcon,
    MatProgressSpinner
  ]
})
export class MaterialModule { }
