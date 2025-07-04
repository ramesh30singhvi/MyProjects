import { Injectable } from '@angular/core';
import { ColumnState } from "ag-grid-community";
import { RolesEnum } from '../models/roles.model';
import { AuthService } from './auth.service';

const KEYS: { [key: string]: string } = {
  COLUMN_STATE: 'column_state',
  FILTER_MODEL: 'filter_model',
  SEARCH_TERM: 'search_term',
  INCLUDE_ARCHIVED: 'include_archived',
  HIDE_ASSIGNED: 'hide_assigned',
  ROW_INDEX: 'row_index'
};

@Injectable()
export class LocalStorageService {
  constructor(private authService: AuthService) { }

  public getColumnState(): ColumnState[] {
    return this.getObject(KEYS.COLUMN_STATE);
  }

  public setColumnState(columnState: ColumnState[]) {
    this.setObject(KEYS.COLUMN_STATE, columnState);
  }

  public getFilterModel(): { [key: string]: any } {
    return this.getObject(KEYS.FILTER_MODEL);
  }

  public setFilterModel(filterModel: { [key: string]: any }): void {
    this.setObject(KEYS.FILTER_MODEL, filterModel);
  }

  public getSearchTerm(): string {
    return this.get(KEYS.SEARCH_TERM);
  }

  public setSearchTerm(searchTerm: string): void {
    this.set(KEYS.SEARCH_TERM, searchTerm);
  }

  public getHideAssigned(): boolean {
    return this.getBoolean(KEYS.HIDE_ASSIGNED);
  }

  public setHideAssigned(hideAssigned: boolean): void {
    this.setBoolean(KEYS.HIDE_ASSIGNED, hideAssigned);
  }

  public getRowIndex(): number {
    return this.getNumber(KEYS.ROW_INDEX);
  }

  public setRowIndex(rowIndex: number): void {
    this.setNumber(KEYS.ROW_INDEX, rowIndex);
  }

  private getObject<T extends object>(key: string): T {
    const dataJSON = this.get(key);
    return JSON.parse(dataJSON);
  }

  private setObject(key: string, data: object): void {
    const dataJSON = JSON.stringify(data);
    this.set(key, dataJSON);
  }

  private getBoolean(key: string): boolean {
    const dataString = this.get(key);
    // converting string to boolean
    return dataString === "true";
  }

  private setBoolean(key: string, data: boolean): void {
    const dataString = data?.toString();
    this.set(key, dataString);
  }

  private getNumber(key: string): number {
    const dataString = this.get(key);
    return parseInt(dataString);
  }

  private setNumber(key: string, value: number): void {
    const dataString = value.toString();
    this.set(key, dataString);
  }

  private get(baseKey: string): string {
    return localStorage.getItem(baseKey);
  }

  private set(baseKey: string, value: string): void {
    localStorage.setItem(baseKey, value);
  }
}