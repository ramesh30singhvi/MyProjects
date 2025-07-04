export interface FallReport {
  byMonth: ByMonth[];
  byActivity: Value[];
  byPlace: Value[];
  byShift: ByShift[];
}

interface ByDay {
  monday: number;
  tuesday: number;
  wednesday: number;
  thursday: number;
  friday: number;
  saturday: number;
  sunday: number;
}

interface ByMonth {
  name: string;
  total: number;
  majorInjury: number;
  sentToHospital: number;
  byDay: ByDay;
}

interface ByShift {
  name: string;
  byTime: Value[];
}

interface Value {
  name: string;
  count: number;
}
