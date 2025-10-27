import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

// Material imports
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';

// Components
import { PasswordListComponent } from './password-list/password-list.component';
import { AddPasswordComponent } from './add-password/add-password.component';
import { ViewPasswordComponent } from './view-password/view-password.component';
import { SharePasswordComponent } from './share-password/share-password.component';
import { ViewSharesComponent } from './view-shares/view-shares.component';

const routes: Routes = [
  {
    path: '',
    component: PasswordListComponent,
  },
];

@NgModule({
  declarations: [
    PasswordListComponent,
    AddPasswordComponent,
    ViewPasswordComponent,
    SharePasswordComponent,
    ViewSharesComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSelectModule,
  ],
})
export class PasswordManagerModule {}
