export interface KeyUsageDto {
  digitalSignature: boolean;
  nonRepudiation: boolean;
  keyEncipherment: boolean;
  dataEncipherment: boolean;
  keyAgreement: boolean;
  keyCertSign: boolean;
  crlSign: boolean;
  encipherOnly: boolean;
  decipherOnly: boolean;
}
