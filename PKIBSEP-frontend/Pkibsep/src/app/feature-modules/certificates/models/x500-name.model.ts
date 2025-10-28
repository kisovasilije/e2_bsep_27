export interface X500NameDto {
  cn: string;        // Common Name
  o?: string;        // Organization
  ou?: string;       // Organizational Unit
  l?: string;        // Locality / City
  st?: string;       // State / Province
  c?: string;        // Country (ISO 3166-1 alpha-2, 2 letters)
}
