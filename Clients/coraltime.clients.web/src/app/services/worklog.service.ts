import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConstantService } from '../core/constant.service';
import { Observable } from 'rxjs';
import { JiraWorklog } from '../models/jira-worklog';

@Injectable()
export class WorkglogService {
  authService: any;

	constructor(private constantService: ConstantService,
	            private http: HttpClient) {
	}

  getWorklogs(filters: any): Observable<any>{
    return this.http.post(this.constantService.jiraWorklog + '/GetWorklogs', filters)
  }

  sendWorklogs(selectedWorklog: JiraWorklog[]): Observable<any>{
    return this.http.post(this.constantService.jiraWorklog + "/LoadTimeEntryWorklogs", selectedWorklog);
  }
}
