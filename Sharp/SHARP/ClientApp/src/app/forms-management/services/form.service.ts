import { Injectable } from "@angular/core";
import { first, Observable } from "rxjs";
import { QuestionGroup } from "src/app/models/audits/questions.model";
import { FormStatuses, IFormStatus } from "src/app/models/forms/forms.model";
import { FormServiceApi } from "src/app/services/form-api.service";

@Injectable()
export class FormService {
    constructor(
        private formServiceApi: FormServiceApi,
      ){}

    public getStatus(statusId: number): IFormStatus {
        const statuses = Object.values(FormStatuses);
    
        return statuses.find((status) => status.id === statusId);
    }

    public isItemsDuplicated(items, keyName): boolean {
        return new Set(items.map((item: any) => item[keyName]?.trim().toLowerCase())).size !== items.length
    }
  public editFormName(formId, formName): Observable<any> {
    return this.formServiceApi.editFormName(formId, formName);
  }
  public duplicateForm(formId, organizationId): Observable<any> {
    return this.formServiceApi.duplicateForm(formId, organizationId);
  }
}
