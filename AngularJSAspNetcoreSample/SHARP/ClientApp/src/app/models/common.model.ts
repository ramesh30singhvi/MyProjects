export enum MessageStatusEnum {
    success,
    info,
    error,
}

export interface IMessageResponse {
    status: MessageStatusEnum,
    message: string;
}