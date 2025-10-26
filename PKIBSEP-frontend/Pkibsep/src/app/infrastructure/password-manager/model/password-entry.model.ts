export interface PasswordEntry {
  id: number;
  siteName: string;
  username: string;
  encryptedPassword: string;
  isOwner: boolean;
  ownerEmail: string;
  createdAt: Date;
  updatedAt?: Date;
}
