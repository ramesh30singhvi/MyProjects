import { IOption } from "./audits/audits.model";

export interface CheckSecureCodeResponse {
  token: string;
  expiration: Date;
  userData: UserData;
}


export interface UserData {
  id: number;
  userId: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: Array<string>;
  organizations: Array<IOption>;
}
