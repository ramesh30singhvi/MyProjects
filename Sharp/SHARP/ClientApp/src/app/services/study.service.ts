import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { StudyDetails } from '../models/study-details.model';
import { StudyGridResponse } from '../models/study.grid.response';
import { StudyFiltersModel } from '../models/studies.filters.model';
import { switchMap } from 'rxjs/operators';

//import * as moment from 'moment-timezone';

@Injectable()
export class StudyService {

  getActiveStudiesUrl = environment.apiUrl + 'studies/search';
  getActiveStudiesFiltersUrl = environment.apiUrl + 'studies/filters?includeArchived={0}&assigned={1}';
  getStudyUrl = environment.apiUrl + 'studies/{id}';
  submitStudyUrl = environment.apiUrl + 'studies/{id}';
  studyReportUrl = environment.apiUrl + 'studies/{id}/report';
  assignUrl = environment.apiUrl + 'studies/{id}/assign';

  constructor(private httpClient: HttpClient) { }

  getActiveStudies(
      startRow,
      endRow,
      sortModel,
      filterModel,
      searchTerm,
      includeArchived,
      detailedSearch: boolean,
      hideAssigned: boolean): Observable<StudyGridResponse> {
    var searchParams = {
      startRow: startRow,
      endRow: endRow,
      colId: null,
      sortOrder: null,
      filter: {},
      detailedSearch: detailedSearch,
      searchTerm: searchTerm,
      includeArchived: includeArchived,
      assigned: !hideAssigned
    }
    var colId = null;
    var sortOrder = null;
    if (sortModel && sortModel.length > 0) {
      colId = sortModel[0].colId;
      sortOrder = sortModel[0].sort;
    }
    if (colId && sortOrder) {
      searchParams.colId = colId;
      searchParams.sortOrder = sortOrder;
    }
    var filterModelObject = JSON.parse(JSON.stringify(filterModel))
    if (filterModelObject.assignedUser) {
      for (var i = 0; i < filterModelObject.assignedUser.values.length; i++) {
        filterModelObject.assignedUser.values[i] = JSON.parse(filterModelObject.assignedUser.values[i]);
      }
    }
    searchParams.filter = filterModelObject;

    return this.httpClient.post<StudyGridResponse>(this.getActiveStudiesUrl, searchParams);
  }
  protected cachedFilter: StudyFiltersModel;
  clearCachedFilters() {
    this.cachedFilter = null;
  }
  getActiveStudiesFilters(includeArchived: boolean, hideAssigned: boolean): Observable<StudyFiltersModel> {
    const url = this.getActiveStudiesFiltersUrl
        .replace('{0}', includeArchived.toString())
        .replace('{1}', (!hideAssigned).toString());
    return this.httpClient.get<StudyFiltersModel>(url).pipe(switchMap(response => {
      this.cachedFilter = response;
      return of<StudyFiltersModel>(response);
    }));
  }
  getStudy(id): Observable<StudyDetails> {
    return this.httpClient.get<StudyDetails>(this.getStudyUrl.replace('{id}',id));
  }

  submitStudy(id, findings, conclusions, isUrgent): Observable<StudyDetails> {
    return this.httpClient.post<StudyDetails>(this.submitStudyUrl.replace('{id}', id), { findings: findings, conclusions: conclusions, isUrgent: isUrgent});
  }
  submitReport(id, findings, conclusions, isUrgent): Observable<StudyDetails> {
    return this.httpClient.post<StudyDetails>(this.studyReportUrl.replace('{id}', id), { findings: findings, conclusions: conclusions, isUrgent: isUrgent });
  }
  assign(id: number, userId: number): Observable<StudyDetails> {
    const url = this.assignUrl.replace('{id}', id.toString());
    return this.httpClient.post<StudyDetails>(url, { userId });
  }
  getReport(id: number): Observable<Object> {
    const url = this.studyReportUrl.replace('{id}', id.toString());
    return this.httpClient.get(url, { responseType: 'blob' });
  }
}
