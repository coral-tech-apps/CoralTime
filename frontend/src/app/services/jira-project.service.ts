import { catchError, map, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable, QueryList } from '@angular/core';
import { ODataServiceFactory, ODataService } from './odata';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';
import { User } from '../models/user';
import { JiraMemberSetting } from '../models/jira-member-setting';

@Injectable()
export class JiraProjectService {
  authService: any;

	constructor(private constantService: ConstantService,
	            private http: HttpClient) {
;
	}

  getAllJiraProjectsBySettingId(id: number): Observable<any[]>{
    return this.http.get<any[]>(this.constantService.jiraProject + `/GetAllJiraProjectsBySettingId?jiraSettingId=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  getNotAssignedProjects(id: number): Observable<any[]>{
    return this.http.get<any[]>(this.constantService.jiraProject + `/GetUnAssignJiraProject?jiraSettingId=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  getAssignedProjects(id: number): Observable<any[]>{
    return this.http.get<any[]>(this.constantService.jiraProject + `/GetAssignJiraProject?jiraSettingId=${id}`).pipe(
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

  loadJiraProjects(domain: string, email: string, token: string): Observable<boolean>{
    return this.http.post<boolean>(this.constantService.jiraProject + `/LoadJiraProject?domain=${domain}&apiToken=${token}&email=${email}`, {}).pipe(
      map(response => {
        return response;
      })
    )
  }
}
