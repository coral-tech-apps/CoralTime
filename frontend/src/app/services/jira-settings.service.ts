import { catchError, map, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable, QueryList } from '@angular/core';
import { ODataServiceFactory, ODataService } from './odata';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';
import { User } from '../models/user';
import { JiraMemberSetting } from '../models/jira-member-setting';

@Injectable()
export class JiraSettingService {
  authService: any;

	constructor(private constantService: ConstantService,
	            private http: HttpClient) {
;
	}

  getAssignedUsers(id: number): Observable<User[]>{
    return this.http.get<User[]>(this.constantService.jiraApi + `/GetAssignedUsers?id=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  getNotAssignedUsers(id: number): Observable<User[]>{
    return this.http.get<User[]>(this.constantService.jiraApi + `/GetNotAssignedUsers?id=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  assignUserToIntegration(memberId: number, jiraSettingId: number): Observable<any>{
    return this.http.post(this.constantService.jiraApi + `/AssignToIntegration?memberId=${memberId}&jiraSettingId=${jiraSettingId}`, {}).pipe(
      map(response => {
        return response;
      })
    )
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

  removeIntegrationFromUser(memberId: number, jiraSettingId: number): Observable<boolean>{
    return this.http.delete<void>(this.constantService.jiraApi + `/UnAssignToIntegration?memberId=${memberId}&jiraSettingId=${jiraSettingId}`).pipe(
      map( () => true)
    )
  }

  loadSettingsTable(id: number): any{
    return this.http.get<any[]>(this.constantService.jiraApi).pipe(
      map(response => {
        return response;
      })
    )
  }

  createNewSetting(jiraSetting: JiraSetting): Observable<any>{
    return this.http.post(this.constantService.jiraApi, jiraSetting)
  }

  updateSetting(jiraSetting: JiraMemberSetting, id: number): Observable<any>{
    const query = this.constantService.jiraApi + `/FillJiraMemberSetting?id=${id}`;
    console.log(query);
    return this.http.patch(query, jiraSetting)
  }

  getJiraMemberSetting(id: number){
  return this.http.get<any[]>(this.constantService.jiraApi + `/GetMemberSettings?id=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }
}
