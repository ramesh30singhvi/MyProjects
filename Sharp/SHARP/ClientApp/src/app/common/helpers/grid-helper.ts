import { IFilterOption } from "src/app/models/audits/audit.filters.model";
import { transformDate, transformNumber } from "./dates-helper";

export const getFilterModelParams = (filterModel: any, filterValues: any) => {
    const filterParams: any = { };

    if (!filterModel) return "";

    Object.keys(filterModel).forEach((key) => {
      const filter = filterModel[key];
      switch (filter.filterType) {
        case "date":
          filterParams[key] = JSON.stringify({
              firstCondition: transformDate(
                filter.condition1 || filter
              ),
              secondCondition: transformDate(filter.condition2),
              operator: filter.operator,
            });
          break;
        case "number":
          filterParams[key] = JSON.stringify({
              firstCondition: transformNumber(
                filter.condition1 || filter
              ),
              secondCondition: transformNumber(filter.condition2),
              operator: filter.operator,
            });
          break;
        default:
          if (filter.values && filter.values.length > 0) {
            const values: IFilterOption[] = filterValues?.[key];

            filterParams[key] = values
              ?.filter((fv: IFilterOption) => filter.values.includes(fv.value));
          }
      }
    });

    return filterParams;
  }