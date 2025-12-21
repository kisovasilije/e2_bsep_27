export function extractCn(subjectDn: string): string | null {
  const cnMatch = subjectDn.match(/CN=([^,]+)/);
  return cnMatch ? cnMatch[1].trim() : null;
}
