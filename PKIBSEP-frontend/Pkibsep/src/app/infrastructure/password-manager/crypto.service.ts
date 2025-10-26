import { Injectable } from '@angular/core';
import * as forge from 'node-forge';
import { KeyPair } from './model/key-pair.model';

@Injectable({
  providedIn: 'root',
})
export class CryptoService {
  /**
   * Generiše RSA par ključeva (2048 bit)
   */
  generateKeyPair(): KeyPair {
    const keypair = forge.pki.rsa.generateKeyPair({ bits: 2048, e: 0x10001 });

    const privateKeyPem = forge.pki.privateKeyToPem(keypair.privateKey);
    const publicKeyPem = forge.pki.publicKeyToPem(keypair.publicKey);

    return {
      privateKeyPem,
      publicKeyPem,
    };
  }

  /**
   * Download-uje privatni ključ kao .pem fajl
   */
  downloadPrivateKey(privateKeyPem: string, filename: string = 'my_private_key.pem'): void {
    const blob = new Blob([privateKeyPem], { type: 'application/x-pem-file' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  /**
   * Učitava privatni ključ iz fajla
   */
  async readPrivateKeyFromFile(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        const content = e.target?.result as string;
        if (this.isValidPrivateKeyPem(content)) {
          resolve(content);
        } else {
          reject(new Error('Neispravan format privatnog ključa'));
        }
      };
      reader.onerror = () => reject(new Error('Greška pri čitanju fajla'));
      reader.readAsText(file);
    });
  }

  /**
   * Enkriptuje lozinku javnim ključem
   */
  encryptPassword(password: string, publicKeyPem: string): string {
    try {
      const publicKey = forge.pki.publicKeyFromPem(publicKeyPem);
      const encrypted = publicKey.encrypt(password, 'RSA-OAEP', {
        md: forge.md.sha256.create(),
      });
      return forge.util.encode64(encrypted);
    } catch (error) {
      console.error('Greška pri enkripciji:', error);
      throw new Error('Greška pri enkripciji lozinke');
    }
  }

  /**
   * Dekriptuje lozinku privatnim ključem
   */
  decryptPassword(encryptedPassword: string, privateKeyPem: string): string {
    try {
      const privateKey = forge.pki.privateKeyFromPem(privateKeyPem);
      const encrypted = forge.util.decode64(encryptedPassword);
      const decrypted = privateKey.decrypt(encrypted, 'RSA-OAEP', {
        md: forge.md.sha256.create(),
      });
      return decrypted;
    } catch (error) {
      console.error('Greška pri dekripciji:', error);
      throw new Error('Greška pri dekripciji lozinke. Proverite da li ste koristili ispravan privatni ključ.');
    }
  }

  /**
   * Re-enkriptuje lozinku za deljenje (dekriptuje sa svojim, enkriptuje sa tuđim)
   */
  reEncryptForSharing(
    encryptedPassword: string,
    myPrivateKeyPem: string,
    targetPublicKeyPem: string
  ): string {
    // Prvo dekriptuj sa svojim privatnim ključem
    const plainPassword = this.decryptPassword(encryptedPassword, myPrivateKeyPem);
    // Zatim enkriptuj sa javnim ključem korisnika sa kojim deliš
    return this.encryptPassword(plainPassword, targetPublicKeyPem);
  }

  /**
   * Validira da li je string validan PEM format privatnog ključa
   */
  isValidPrivateKeyPem(pem: string): boolean {
    return (
      pem.includes('-----BEGIN RSA PRIVATE KEY-----') ||
      pem.includes('-----BEGIN PRIVATE KEY-----')
    );
  }

  /**
   * Validira da li je string validan PEM format javnog ključa
   */
  isValidPublicKeyPem(pem: string): boolean {
    return pem.includes('-----BEGIN PUBLIC KEY-----');
  }

  /**
   * Testira da li privatni i javni ključ odgovaraju jedan drugom
   */
  testKeyPair(privateKeyPem: string, publicKeyPem: string): boolean {
    try {
      const testMessage = 'test';
      const encrypted = this.encryptPassword(testMessage, publicKeyPem);
      const decrypted = this.decryptPassword(encrypted, privateKeyPem);
      return testMessage === decrypted;
    } catch {
      return false;
    }
  }
}
