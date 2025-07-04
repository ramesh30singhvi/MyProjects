import * as moment from "moment";
import { IUserTimeZone } from "src/app/models/audits/audits.model";
import {
  MM_DD_YYYY_HH_MM_A_SLASH,
  MM_DD_YYYY_SLASH,
} from "../constants/date-constants";

export const transformDate = (
  condition: {
    dateFrom: string;
    dateTo: string;
    type: string;
  },
  isUtc: boolean = false
): { from: string; to: string; type: string } => {
  if (!condition) return null;
  const format = "YYYY-MM-DD HH:mm:ss";

  let dateFrom: string;
  if (condition.dateFrom) {
    const date = moment(new Date(condition.dateFrom).toUTCString());
    dateFrom = isUtc ? date.utc().format(format) : date.format(format);
  }

  let dateTo: string;
  if (condition.dateTo) {
    const date = moment(new Date(condition.dateTo).toUTCString());
    dateTo = isUtc ? date.utc().format(format) : date.format(format);
  }

  return {
    from: dateFrom,
    to: dateTo,
    type: condition.type,
  };
};

export const transformNumber = (condition: {
  filter: string;
  filterTo: string;
  type: string;
}): { from: string; to: string; type: string } => {
  if (!condition) return null;

  return {
    from: condition.filter,
    to: condition.filterTo,
    type: condition.type,
  };
};

export const formatGridDate = (date: string) => {
  return date ? moment(date).format(MM_DD_YYYY_SLASH) : date;
};

export const formatGridDateTimeLocal = (date: string) => {
  return date ? moment(date).local().format(MM_DD_YYYY_HH_MM_A_SLASH) : date;
};

export const getUserLocalDateTime = (userTimeZone: IUserTimeZone): string => {
  const currentDateTimeUtc = moment().format(MM_DD_YYYY_HH_MM_A_SLASH);

  const userLocalDateTime = userTimeZone
    ? moment(currentDateTimeUtc, MM_DD_YYYY_HH_MM_A_SLASH)
        .utcOffset(userTimeZone.userTimeZoneInfo.baseUtcOffset)
        .format(MM_DD_YYYY_HH_MM_A_SLASH)
    : currentDateTimeUtc;

  return userLocalDateTime;
};
