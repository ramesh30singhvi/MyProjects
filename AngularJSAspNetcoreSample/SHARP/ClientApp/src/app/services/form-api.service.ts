import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "src/environments/environment";
import { map } from "rxjs/operators";
import { IFormOption, IKeyword, IKeywordTrigger, IOption } from "../models/audits/audits.model";
import { NgxSpinnerService } from "ngx-spinner";
import {
  AddEditForm,
  FormGridItem,
  FormManagementFiltersModel,
  IFormField,
  IFormVersion,
} from "../models/forms/forms.model";
import {
  IEditQuestion,
  IEditTrackerQuestion,
  IGroup,
  ISection,
  Question,
  Section,
} from "../models/audits/questions.model";
import { transformDate } from "../common/helpers/dates-helper";
import { IFilterOption } from "../models/audits/audit.filters.model";

@Injectable()
export class FormServiceApi {
  formsAdminUrl = environment.apiUrl + "forms/admin";
  private formsUrl = `${environment.apiUrl}forms`;

  constructor(
    private httpClient: HttpClient,
    private spinner: NgxSpinnerService
  ) {}

  public getFormsForKeywordTrigger(organizationid: any,keyword:string) {
    return this.httpClient
      .get<IOption[]>(`${this.formsAdminUrl}/formsTriggered/${organizationid}/${keyword}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public getForms(
    search: string,
    filterModel: any,
    sortModel: any[],
    startRow: number,
    endRow: number,
    formManagementFilterValues: FormManagementFiltersModel
  ): Observable<FormGridItem[]> {
    this.spinner.show();
    const filters = this.getFilterModelParams(
      filterModel,
      formManagementFilterValues
    );

    var searchParams = {
      search,
      skipCount: startRow,
      takeCount: endRow - startRow,
      orderBy: "",
      sortOrder: "",
    };

    if (sortModel?.length) {
      const { colId, sort } = sortModel[0];
      searchParams.orderBy = colId;
      searchParams.sortOrder = sort;
    }

    return this.httpClient.post<FormGridItem[]>(
      `${this.formsAdminUrl}/versions/get`,
      {
        ...searchParams,
        ...filters,
      }
    );
  }

  public getFormsFilters(
    column: string,
    filterModel: any,
    formManagementFilterValues: FormManagementFiltersModel
  ): Observable<IFilterOption[]> {
    const filters = this.getFilterModelParams(
      filterModel,
      formManagementFilterValues
    );
    return this.httpClient.post<IFilterOption[]>(
      `${this.formsAdminUrl}/versions/filters`,
      {
        column,
        formManagementFilter: filters,
      }
    );
  }

  public getFormFilteredOptions(
    search: string,
    skipCount: number,
    takeCount: number,
    organizationIds: number[] | null
  ): Observable<IOption[]> {
    return this.httpClient.post<IOption[]>(`${this.formsUrl}/options`, {
      search,
      skipCount,
      takeCount,
      organizationIds,
    });
  }

  public getAuditTypeOptions(): Observable<IOption[]> {
    return this.httpClient.get<IOption[]>(`${this.formsUrl}/types`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }
  public getAuditTypeOptionsExcluded(
    excludedTypes: Array<string>
  ): Observable<IOption[]> {
    return this.httpClient.get<IOption[]>(`${this.formsUrl}/types`).pipe(
      map((data: IOption[]) => {
        return data.filter(
          (a) => excludedTypes.indexOf(a.name.toLowerCase()) == -1
        );
      })
    );
  }

  public getFormVersion(formVersionId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient.get<any>(`${this.formsUrl}/${formVersionId}`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public getFormVersions(formVersionId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient.get<any>(`${this.formsUrl}/${formVersionId}/versions`).pipe(
      map((data: any) => {
        return data;
      })
    );
  }

  public getQuestions(formVersionIds: number[]): Observable<any> {
    this.spinner.show();
    return this.httpClient
      .post<any>(`${this.formsUrl}/questions`, {
        formVersionIds,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addForm(addEditForm: AddEditForm): Observable<IFormVersion> {
    this.spinner.show();

    return this.httpClient
      .post<IFormVersion>(this.formsAdminUrl, {
        name: addEditForm.name,
        auditTypeId: addEditForm.auditType.id,
        organizationId: addEditForm.organization.id,
        allowEmptyComment: addEditForm.allowEmptyComment,
        disableCompliance: addEditForm.disableCompliance
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }


  public publishForm(formVersionId: number, allowEmptyComment: number, disableCompliance: number, useHighAlert: boolean,ahTime:number): Observable<boolean> {
    this.spinner.show();
    console.log(ahTime);
    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/publish/${formVersionId}/${allowEmptyComment}/${disableCompliance}/${useHighAlert}/${ahTime}`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editForm(formVersionId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .post<any>(`${this.formsAdminUrl}/${formVersionId}`, {})
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editFormName(formId: number, formName: string): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .post<any>(`${this.formsAdminUrl}/form/${formId}`, { formName })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public duplicateForm(formId: number, organizationId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .post<any>(`${this.formsAdminUrl}/form/${formId}/duplicate/${organizationId}`, {  })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public deleteForm(formVersionId: number): Observable<any> {
    this.spinner.show();

    return this.httpClient
      .delete<any>(`${this.formsAdminUrl}/${formVersionId}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addFormKeyword(
    formVersionId: number,
    keyword: string,
    trigger: boolean,
    formsTrigged: any[]
  ): Observable<IKeywordTrigger> {
    this.spinner.show();
  
    return this.httpClient
      .post<IKeywordTrigger>(`${this.formsAdminUrl}/keyword`, {
        formVersionId,
        name: keyword,
        trigger: trigger,
        formsTriggeredByKeyword: formsTrigged
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editFormKeyword(
    id: number,
    formVersionId: number,
    keyword: string,
    trigger: boolean,
    formsTrigged: any[]
  ): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/keyword/${id}`, {
        formVersionId,
        name: keyword,
        trigger: trigger,
        formsTriggeredByKeyword: formsTrigged
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public deleteColumnKeyword(id: number): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .delete<boolean>(`${this.formsAdminUrl}/columnkeyword/${id}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }
  public deleteColumn(id: number): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .delete<boolean>(`${this.formsAdminUrl}/column/${id}`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public rearrangeQuestions(questions: any[]): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/questions/rearrange`, { questions })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getSectionOptions(formVersionId: number): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.formsAdminUrl}/${formVersionId}/sections`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getMdsSections(formVersionId: number): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.formsAdminUrl}/${formVersionId}/mdssections`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getFieldTypeOptions(): Observable<IOption[]> {
    return this.httpClient
      .get<IOption[]>(`${this.formsAdminUrl}/field/types`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public addQuestion(question: IEditQuestion): Observable<Question> {
    this.spinner.show("criteriaQuestionEditSpinner");

    return this.httpClient
      .post<Question>(`${this.formsAdminUrl}/criteria/question`, {
        id: question.id,
        question: question.question,
        formVersionId: question.formVersionId,
        groupId: question.group?.id,
        groupName: question.group?.name,
        parentId: question.parentId,
        showNA: question.showNA,
        compliance: question.compliance,
        priority: question.priority,
        quality: question.quality,
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("criteriaQuestionEditSpinner");
          return data;
        })
      );
  }

  public editQuestion(question: IEditQuestion): Observable<Question> {
    this.spinner.show("criteriaQuestionEditSpinner");

    return this.httpClient
      .put<Question>(`${this.formsAdminUrl}/criteria/question`, {
        id: question.id,
        question: question.question,
        formVersionId: question.formVersionId,
        groupId: question.group?.id,
        groupName: question.group?.name,
        parentId: question.parentId,
        showNA: question.showNA,
        compliance: question.compliance,
        priority: question.priority,
        quality: question.quality,
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("criteriaQuestionEditSpinner");
          return data;
        })
      );
  }

  public addTrackerQuestion(
    question: IEditTrackerQuestion
  ): Observable<Question> {
    this.spinner.show("trackerQuestionEditSpinner");

    return this.httpClient
      .post<Question>(`${this.formsAdminUrl}/tracker/question`, {
        question: question.question,
        formVersionId: question.formVersionId,
        fieldTypeId: question.fieldType?.id,
        isRequired: question.isRequired,
        compliance: question.compliance,
        priority: question.priority,
        quality: question.quality,
        items: question.items,
        formGroupId: question.formGroupId
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("trackerQuestionEditSpinner");
          return data;
        })
      );
  }

  public addMdsQuestion(
    question: IFormField
  ): Observable<Question> {
    this.spinner.show("trackerQuestionEditSpinner");

    return this.httpClient
      .post<Question>(`${this.formsAdminUrl}/mds/question`, {
        question: question.labelName,
        formVersionId: question.formVersionId,
        fieldTypeId: question.fieldType?.id,
        isRequired: question.isRequired,
        items: question.items,
        formGroupId: question.formGroupId
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("trackerQuestionEditSpinner");
          return data;
        })
      );
  }

  public editMdsQuestion(
    question: any
  ): Observable<Question> {
    this.spinner.show("trackerQuestionEditSpinner");

          console.log("question", question);


    return this.httpClient
      .put<Question>(`${this.formsAdminUrl}/mds/question`, {
        id: question.id,
        question: question.labelName,
        fieldTypeId: question.fieldType?.id,
        isRequired: question.isRequired,
        items: question.items,
        formVersionId: question.formVersionId
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("trackerQuestionEditSpinner");
          return data;
        })
      );
  }

  public addSection(section: ISection): Observable<Section> {
    this.spinner.show("mdsSectionEditSpinner")

    return this.httpClient
    .post<Section>(`${this.formsAdminUrl}/mds/section`, {
      name: section.name,
      formVersionId: section.formVersionId
    })
    .pipe(
      map((data: any) => {
        this.spinner.hide("mdsSectionEditSpinner");
        return data;
      })
    );
  }

  public editSection(section: ISection): Observable<Section> {
    this.spinner.show("mdsSectionEditSpinner")
    return this.httpClient
      .put<Section>(`${this.formsAdminUrl}/mds/section`, {
        id: section.id,
        name: section.name
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("mdsSectionEditSpinner");
          return data;
        })
      );
  }

  public addGroup(group: IGroup): Observable<Section> {
    this.spinner.show("mdsGroupEditSpinner")

    return this.httpClient
      .post<Section>(`${this.formsAdminUrl}/mds/group`, {
        name: group.name,
        formVersionId: group.formVersionId,
        formSectionId: group.formSectionId
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("mdsGroupEditSpinner");
          return data;
        })
      );
  }

  public editGroup(group: IGroup): Observable<Section> {
    this.spinner.show("mdsGroupEditSpinner")

    return this.httpClient
      .put<Section>(`${this.formsAdminUrl}/mds/group`, {
        id: group.id,
        name: group.name
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("mdsGroupEditSpinner");
          return data;
        })
      );
  }

  public editTrackerQuestion(
    question: IEditTrackerQuestion
  ): Observable<Question> {
    this.spinner.show("trackerQuestionEditSpinner");

    return this.httpClient
      .put<Question>(`${this.formsAdminUrl}/tracker/question`, {
        id: question.id,
        question: question.question,
        formVersionId: question.formVersionId,
        fieldTypeId: question.fieldType?.id,
        isRequired: question.isRequired,
        compliance: question.compliance,
        priority: question.priority,
        quality: question.quality,
        items: question.items,
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("trackerQuestionEditSpinner");
          return data;
        })
      );
  }



  public addFormField(
    formVersionId: number,
    formField: IFormField
  ): Observable<Question> {
    this.spinner.show("criteriaSubheaderEditSpinner");

    return this.httpClient
      .post<Question>(`${this.formsAdminUrl}/fields`, {
        formVersionId,
        fieldTypeId: formField.fieldType?.id,
        fieldName: formField.fieldName,
        labelName: formField.labelName,
        isRequired: formField.isRequired,

        items: formField.items,
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("criteriaSubheaderEditSpinner");
          return data;
        })
      );
  }

  public editFormField(
    formVersionId: number,
    formField: IFormField
  ): Observable<Question> {
    this.spinner.show("criteriaSubheaderEditSpinner");

    return this.httpClient
      .put<Question>(`${this.formsAdminUrl}/fields`, {
        id: formField.id,
        formVersionId,
        fieldTypeId: formField.fieldType?.id,
        fieldName: formField.fieldName,
        labelName: formField.labelName,
        isRequired: formField.isRequired,

        items: formField.items,
      })
      .pipe(
        map((data: any) => {
          this.spinner.hide("criteriaSubheaderEditSpinner");
          return data;
        })
      );
  }

  public deleteFormField(
    formVersionId: number,
    fieldId: number
  ): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .delete<boolean>(
        `${this.formsAdminUrl}/${formVersionId}/fields/${fieldId}`
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public rearrangeFormFields(
    formVersionId: number,
    items: any[]
  ): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/${formVersionId}/fields/rearrange`, {
        items,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public editFormSections(
    formVersionId: number,
    sections: any[]
  ): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/${formVersionId}/sections`, {
        sections,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getFormVersionOptions(
    organizationId: number,
    auditType?: string
  ): Observable<IFormOption[]> {
    return this.httpClient
      .get<IFormOption[]>(
        `${this.formsUrl}/organization/${organizationId}/versions`,
        { params: auditType ? { auditType } : null }
      )
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getFormOptions(
    organizationId: number,
    auditType?: string
  ): Observable<IFormOption[]> {
    return this.httpClient
      .get<IFormOption[]>(`${this.formsUrl}/organization/${organizationId}`, {
        params: auditType ? { auditType } : null,
      })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public setFormState(formId: number, state: boolean): Observable<boolean> {
    this.spinner.show();

    return this.httpClient
      .put<boolean>(`${this.formsAdminUrl}/${formId}/state`, { state })
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  public getLastActiveFormVersion(formId: number): Observable<number> {
    this.spinner.show();

    return this.httpClient
      .get<number>(`${this.formsAdminUrl}/${formId}/last/version`)
      .pipe(
        map((data: any) => {
          return data;
        })
      );
  }

  getFilterModelParams(
    filterModel: any,
    formManagementFilterValues: FormManagementFiltersModel
  ) {
    const filterParams: any = {};

    if (!filterModel) return "";

    Object.keys(filterModel).forEach((key) => {
      const filter = filterModel[key];
      switch (filter.filterType) {
        case "date":
          filterParams[key] = JSON.stringify({
            firstCondition: transformDate(filter.condition1 || filter),
            secondCondition: transformDate(filter.condition2),
            operator: filter.operator,
          });
          break;
        default:
          if (filter.values && filter.values.length > 0) {
            const filterValues: IFilterOption[] =
              formManagementFilterValues?.[key];

            filterParams[key] = filterValues?.filter((fv: IFilterOption) =>
              filter.values.includes(fv.value)
            );
          }
      }
    });

    return filterParams;
  }
}
