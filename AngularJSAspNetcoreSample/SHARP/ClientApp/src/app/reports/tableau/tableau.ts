export interface ITableau {
  getWorkbook(): any
  showExportPDFDialog(): any
  showExportCrossTabDialog(): any
}

export enum FilterUpdateType {
  Replace = "REPLACE"
}
