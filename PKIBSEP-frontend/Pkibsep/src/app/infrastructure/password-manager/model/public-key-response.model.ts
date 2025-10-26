export interface PublicKeyResponse {
  publicKeyPem: string | null;
  keyGeneratedAt: Date | null;
  hasKey: boolean;
}
