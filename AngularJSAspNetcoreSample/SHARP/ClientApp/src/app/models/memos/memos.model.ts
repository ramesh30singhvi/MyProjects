import { IOption, IUserOption } from "../audits/audits.model";

export interface IMemo {
    id: number,
    user: IUserOption,
    organizations: IOption[],
    text: string,
    createdDate: Date,
    validityDate: Date,
}

export interface IEditMemo {
    id: number,
    organizationIds: IOption[],
    text: string,
    validityDate: string,
}