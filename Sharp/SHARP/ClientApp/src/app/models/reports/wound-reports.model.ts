export interface WoundReport {
  byMonths: ByMonth[];
}

interface ByMonth {
  name: string;
  total: number;
  inHouseAcquired: number;
  reHospitalization: number;
  byTypes: ByType[];
}

interface ByType {
  name: string;
  count: number;
}
