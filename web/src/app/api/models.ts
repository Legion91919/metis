/** Shapes returned by the Metis API (see GET /api/queue and GET /api/criteria). */
export interface Mover { key: string; criterion: string; evidence: string; points: number; }

export interface QueueItem {
  rank: number; applicationId: string; dealName: string; sponsor: string; assetType: string; region: string;
  loanAmount: number; currency: string; loanToValue: number; debtServiceCover: number; score: number;
  movers: Mover[]; explanation: string; waitingHours: number; arrivalPosition: number;
}

export interface QueueResponse { criteriaVersion: string; asOf: string; total: number; items: QueueItem[]; }

export interface CriterionWeight { key: string; name: string; weight: number; share: number; }

export interface CriteriaResponse { version: string; criteria: CriterionWeight[]; lastReloadError: string | null; }

export interface LoginResponse { tokenType: string; accessToken: string; expiresIn: number; refreshToken: string; }
