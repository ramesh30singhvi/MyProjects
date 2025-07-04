export enum UserStatus {
  Active = 1,
  Inactive = 2
};

export const UserStatusOptions = {
  [UserStatus.Active]: {
    name: "Active",
    color: "#43910A",
    id: 1
  },
  [UserStatus.Inactive]: {
    name: "Inactive",
    color: "#F62D51",
    id: 2
  },
};