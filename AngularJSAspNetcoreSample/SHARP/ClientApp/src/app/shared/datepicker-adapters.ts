import { Injectable} from '@angular/core';
import { NgbDateAdapter, NgbDateParserFormatter, NgbDateStruct} from '@ng-bootstrap/ng-bootstrap';

@Injectable()
export class CustomDateParserFormatter extends NgbDateParserFormatter {

  readonly DELIMITER = '.';

  parse(value: string): NgbDateStruct | null {
    if (value) {
      const date = value.split(this.DELIMITER);
      return {
        month : parseInt(date[0], 10),
        day : parseInt(date[1], 10),
        year : parseInt(date[2], 10)
      };
    }
    return null;    
  }

  format(ngDate: NgbDateStruct | null): string {
    if(!ngDate){
      return '';
    }

    return `${ngDate.month.toString().padStart(2, '0')}.${ngDate.day.toString().padStart(2, '0')}.${ngDate.year}`;
  }
}

@Injectable()
export class CustomDateParserAdapter extends NgbDateAdapter<string> {

  readonly DELIMITER = '.';

  fromModel(value: string | null): NgbDateStruct | null {
    if (value) {
      const date = value.split(this.DELIMITER);
      return {
        month : parseInt(date[0], 10),
        day : parseInt(date[1], 10),        
        year : parseInt(date[2], 10)
      };
    }
    return null;
  }

  toModel(date: NgbDateStruct | null): string | null {
    return date ? `${date.month.toString().padStart(2, '0')}.${date.day.toString().padStart(2, '0')}.${date.year}`: null;
  }
}