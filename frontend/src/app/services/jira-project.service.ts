import { catchError, map, Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable, QueryList } from '@angular/core';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';
import { User } from '../models/user';
import { JiraMemberSetting } from '../models/jira-member-setting';
import { ODataServiceFactory, PagedResult } from './odata';
import { NotAssignedJiraProject } from '../models/not-assigned-jira-project';
import { AssignedJiraProject } from '../models/assigned-jira-project';

@Injectable()
export class JiraProjectService {
  authService: any;

	constructor(private constantService: ConstantService,
              private odataFactory: ODataServiceFactory,
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

  getNotAssignedProjects(id: number, event, filterStr = ''): any{
    let odata = this.odataFactory.CreateService<NotAssignedJiraProject>('JiraProject/GetUnAssignJiraProject/'+id);
    let filters: string[] = []

    let query = odata
      .Query();

    if (event.sortField) {
      query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    } else {
      query.OrderBy('name' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    }
    if(filterStr){
      filters.push('contains(tolower(name),\'' + filterStr.trim().toLowerCase() + '\')');
    }

    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new NotAssignedJiraProject(x));
			return res;
		}));
  }

  getAssignedProjects(id: number, event, filterStr = ''): any{
    let odata = this.odataFactory.CreateService<AssignedJiraProject>('JiraProject/GetAssignJiraProject/'+id);
    let filters: string[] = []

    let query = odata
      .Query();

    if (event.sortField) {
      query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    } else {
      query.OrderBy('name' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    }
    if(filterStr){
      filters.push('contains(tolower(jiraProjectName),\'' + filterStr.trim().toLowerCase() + '\')');
    }

    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new AssignedJiraProject(x));
			return res;
		}));
  }

  assignUserToIntegration(memberId: number, jiraSettingId: number): Observable<any>{
    return this.http.post(this.constantService.jiraApi + `/AssignToIntegration?memberId=${memberId}&jiraSettingId=${jiraSettingId}`, {}).pipe(
      map(response => {
        return response;
      })
    )
  }

  loadJiraProjects(id: number): Observable<boolean>{
    return this.http.post<boolean>(this.constantService.jiraProject + `/LoadJiraProject?id=${id}`, {}).pipe(
      map(response => {
        return response;
      })
    )
  }

  linkProjects(projectId: number, jiraProjectId: number): Observable<any>{
    return this.http.post(this.constantService.jiraProject + `/LinkProjects?projectId=${projectId}&jiraProjectId=${jiraProjectId}`, {}).pipe(
      map(response => {
        return response;
      })
    )
  }

  removeProjectJiraLink(id: number): Observable<any>{
    return this.http.delete(this.constantService.jiraProject + `/RemoveProjectJiraLink?id=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }
}
