export interface UserPublicKey {
  userId: number;
  email: string;
  publicKeyPem: string | null;
  hasKey: boolean;
}
