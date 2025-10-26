import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileComponent } from './profile/profile.component';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule } from '@angular/material/dialog';
import { SharedModule } from 'src/app/shared/shared.module';
import { KeySetupComponent } from './key-setup/key-setup.component';
import { ConfirmDialogComponent } from './key-setup/confirm-dialog/confirm-dialog.component';

@NgModule({
  declarations: [ProfileComponent, KeySetupComponent, ConfirmDialogComponent],
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDialogModule, SharedModule],
  exports: [ProfileComponent],
})
export class ProfileModule {}
