export interface Session {
  readonly id: number;
  readonly userId: number;
  readonly ipAddress: string;
  readonly userAgent: string;
  readonly lastActive: Date;
}
