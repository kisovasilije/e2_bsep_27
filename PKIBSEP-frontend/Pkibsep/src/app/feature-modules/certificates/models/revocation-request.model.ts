export interface RevocationRequest {
  certificateId: number;

  /**
   * Reason for revocation as per RFC 5280 Section 5.3.1
   * https://datatracker.ietf.org/doc/html/rfc5280#section-5.3.1.
   * It is the number code that represents the reason.
   */
  reason: number;

  comment: string | undefined | null;
}
