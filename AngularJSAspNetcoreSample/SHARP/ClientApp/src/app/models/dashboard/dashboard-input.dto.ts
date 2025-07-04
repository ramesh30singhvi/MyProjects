export type AddTableDto = {
  organizationId: number;
  name: string;
};

export type AddGroupDto = {
  organizationId: number;
  tableId: number | undefined;
  name: string;
};

export type AddElementDto = {
  organizationId: number;
  groupId: number | undefined;
  name: string;
  keyword: string;
  formId?: number;
};

export type EditDashboardInputDto = {
  organizationId: number;
  id: number;
  name: string;
  keyword: string;
  formId?: number;
};
