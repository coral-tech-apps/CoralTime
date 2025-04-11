import { first, filter } from 'rxjs/operators';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable, QueryList } from '@angular/core';
import { ODataServiceFactory, ODataService } from './odata';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';
import { User } from '../models/user';
import { JiraMemberSetting } from '../models/jira-member-setting';
import { Project } from '../models/project';

@Injectable()
export class JiraSettingService {
  authService: any;

	constructor(private constantService: ConstantService,
              private odataFactory: ODataServiceFactory,
	            private http: HttpClient) {
;
	}

  getAssignedUsers(id: number, filterStr: string = ''): any{
    let odata = this.odataFactory.CreateService<User>('jira/GetAssignedUsers/'+id);
    let filters: string[] = []

    if(filterStr){
      filters.push('contains(tolower(fullName),\'' + filterStr.trim().toLowerCase() + '\')');
    }

    let query = odata
      .Query();
    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new User(x));
			return res;
		}));
  }

  getNotAssignedUsers(id: number, filterStr: string = ''): any{
    let odata = this.odataFactory.CreateService<User>('jira/GetNotAssignedUsers/'+id);
    let filters: string[] = []

    if(filterStr){
      filters.push('contains(tolower(fullName),\'' + filterStr.trim().toLowerCase() + '\')');
    }

    let query = odata
      .Query();
    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new User(x));
			return res;
		}));
  }

  assignUserToIntegration(memberId: number, jiraSettingId: number): Observable<any>{
    return this.http.post(this.constantService.jiraApi + `/AssignToIntegration?memberId=${memberId}&jiraSettingId=${jiraSettingId}`, {}).pipe(
      map((response) => {
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

  loadSettingsTable(id: number, filterStr: string = ''): any{
    let odata = this.odataFactory.CreateService<JiraSetting>('Jira');
    let filters: string[] = []

    if(filterStr){
      filters.push('contains(tolower(settingName),\'' + filterStr.trim().toLowerCase() + '\')');
    }

    let query = odata
      .Query();

    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new JiraSetting(x));
			return res;
		}));
  }

  createNewSetting(jiraSetting: JiraSetting): Observable<any>{
    return this.http.post(this.constantService.jiraApi, jiraSetting)
  }

  updateSetting(jiraSetting: JiraSetting, id: number): Observable<any>{
    const query = this.constantService.jiraApi + `/?id=${id}`;
    return this.http.patch(query, jiraSetting)
  }

  fillJiraMemberSetting(jiraSetting: JiraMemberSetting, id: number): Observable<any>{
    const query = this.constantService.jiraApi + `/FillJiraMemberSetting?id=${id}`;
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
