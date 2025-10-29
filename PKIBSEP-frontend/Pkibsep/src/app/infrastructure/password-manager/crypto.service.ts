import { Injectable } from '@angular/core';
import { KeyPair } from './model/key-pair.model';

@Injectable({
  providedIn: 'root',
})
export class CryptoService {

  async generateKeyPair(): Promise<KeyPair> {
    const keyPair = await window.crypto.subtle.generateKey(
      {
        name: 'RSA-OAEP',
        modulusLength: 2048,
        publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537
        hash: 'SHA-256',
      },
      true, // extractable
      ['encrypt', 'decrypt']
    );

    const privateKeyPem = await this.exportPrivateKeyToPem(keyPair.privateKey);
    const publicKeyPem = await this.exportPublicKeyToPem(keyPair.publicKey);

    return {
      privateKeyPem,
      publicKeyPem,
    };
  }

  private async exportPrivateKeyToPem(privateKey: CryptoKey): Promise<string> {
    const exported = await window.crypto.subtle.exportKey('pkcs8', privateKey);
    const exportedAsBase64 = this.arrayBufferToBase64(exported);
    return `-----BEGIN PRIVATE KEY-----\n${this.formatPem(exportedAsBase64)}\n-----END PRIVATE KEY-----`;
  }

  private async exportPublicKeyToPem(publicKey: CryptoKey): Promise<string> {
    const exported = await window.crypto.subtle.exportKey('spki', publicKey);
    const exportedAsBase64 = this.arrayBufferToBase64(exported);
    return `-----BEGIN PUBLIC KEY-----\n${this.formatPem(exportedAsBase64)}\n-----END PUBLIC KEY-----`;
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }

  private formatPem(base64: string): string {
    return base64.match(/.{1,64}/g)?.join('\n') || base64;
  }

  downloadPrivateKey(privateKeyPem: string, filename: string = 'my_private_key.pem'): void {
    const blob = new Blob([privateKeyPem], { type: 'application/x-pem-file' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
    URL.revokeObjectURL(link.href);
  }

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

  private async importPublicKey(publicKeyPem: string): Promise<CryptoKey> {
    const pemContents = publicKeyPem
      .replace('-----BEGIN PUBLIC KEY-----', '')
      .replace('-----END PUBLIC KEY-----', '')
      .replace(/\s/g, '');

    const binaryDer = this.base64ToArrayBuffer(pemContents);

    return await window.crypto.subtle.importKey(
      'spki',
      binaryDer,
      {
        name: 'RSA-OAEP',
        hash: 'SHA-256',
      },
      true,
      ['encrypt']
    );
  }

  /**
   * Importuje privatni ključ iz PEM formata
   */
  private async importPrivateKey(privateKeyPem: string): Promise<CryptoKey> {
    // Ukloni header/footer i whitespace
    let pemContents = privateKeyPem
      .replace('-----BEGIN PRIVATE KEY-----', '')
      .replace('-----END PRIVATE KEY-----', '')
      .replace('-----BEGIN RSA PRIVATE KEY-----', '')
      .replace('-----END RSA PRIVATE KEY-----', '')
      .replace(/\s/g, '');

    const binaryDer = this.base64ToArrayBuffer(pemContents);

    return await window.crypto.subtle.importKey(
      'pkcs8',
      binaryDer,
      {
        name: 'RSA-OAEP',
        hash: 'SHA-256',
      },
      true,
      ['decrypt']
    );
  }

  private base64ToArrayBuffer(base64: string): ArrayBuffer {
    const binaryString = window.atob(base64);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
  }

  async encryptPassword(password: string, publicKeyPem: string): Promise<string> {
    try {
      const publicKey = await this.importPublicKey(publicKeyPem);
      const encoder = new TextEncoder();
      const data = encoder.encode(password);

      const encrypted = await window.crypto.subtle.encrypt(
        {
          name: 'RSA-OAEP',
        },
        publicKey,
        data
      );

      return this.arrayBufferToBase64(encrypted);
    } catch (error) {
      console.error('Greška pri enkripciji:', error);
      throw new Error('Greška pri enkripciji lozinke');
    }
  }

  async decryptPassword(encryptedPassword: string, privateKeyPem: string): Promise<string> {
    try {
      const privateKey = await this.importPrivateKey(privateKeyPem);
      const encryptedData = this.base64ToArrayBuffer(encryptedPassword);

      const decrypted = await window.crypto.subtle.decrypt(
        {
          name: 'RSA-OAEP',
        },
        privateKey,
        encryptedData
      );

      const decoder = new TextDecoder();
      return decoder.decode(decrypted);
    } catch (error) {
      console.error('Greška pri dekripciji:', error);
      throw new Error('Greška pri dekripciji lozinke. Proverite da li ste koristili ispravan privatni ključ.');
    }
  }

  async reEncryptForSharing(
    encryptedPassword: string,
    myPrivateKeyPem: string,
    targetPublicKeyPem: string
  ): Promise<string> {

    const plainPassword = await this.decryptPassword(encryptedPassword, myPrivateKeyPem);
    return await this.encryptPassword(plainPassword, targetPublicKeyPem);
  }

  isValidPrivateKeyPem(pem: string): boolean {
    return (
      pem.includes('-----BEGIN RSA PRIVATE KEY-----') ||
      pem.includes('-----BEGIN PRIVATE KEY-----')
    );
  }

  isValidPublicKeyPem(pem: string): boolean {
    return pem.includes('-----BEGIN PUBLIC KEY-----');
  }
}
