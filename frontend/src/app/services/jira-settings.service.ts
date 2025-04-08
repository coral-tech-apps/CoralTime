import { catchError, map, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable, QueryList } from '@angular/core';
import { ODataServiceFactory, ODataService } from './odata';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';
import { User } from '../models/user';

@Injectable()
export class JiraSettingService {
  authService: any;

	constructor(private constantService: ConstantService,
	            private http: HttpClient) {
;
	}

  isEnableJira(): Observable<boolean>{
    return this.http.get<boolean>(this.constantService.apiBaseUrl + '/odata/Members/IsJiraEnable').pipe(
      map(response => {
        return response;
      })
    )
  }

  changeJiraStatus(status: boolean): Observable<boolean>{
    return this.http.post<boolean>(this.constantService.apiBaseUrl + `/odata/Members/ChangeJiraField?jiraSatus=${status}`, {}).pipe(
        map(response => {
          return response;
        })
      )
  }

  deleteSetting(index: string): Observable<boolean>{
    return this.http.delete<void>(this.constantService.jiraApi+`?id=${index}`).pipe(
      map(() => true),
      catchError(error => {
        return of(false);
      })
    )
  }

  loadSettingsTable(id: number): any{
    return this.http.get<any[]>(this.constantService.jiraApi + `?memberId=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  createNewSetting(jiraSetting: JiraSetting): Observable<any>{
    return this.http.post(this.constantService.jiraApi, jiraSetting)
  }

  updateSetting(jiraSetting: JiraSetting, id: string): Observable<any>{
    const query = this.constantService.jiraApi + `?id=${id}`;

    return this.http.patch(query, jiraSetting)
  }
}
