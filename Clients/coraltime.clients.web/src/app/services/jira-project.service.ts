import { map, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConstantService } from '../core/constant.service';
import { ODataServiceFactory, PagedResult } from './odata';
import { NotAssignedJiraProject } from '../models/not-assigned-jira-project';
import { AssignedJiraProject } from '../models/assigned-jira-project';
import { TableLazyLoadEvent } from 'primeng/table';

@Injectable()
export class JiraProjectService {
  authService: any;

	constructor(private constantService: ConstantService,
              private odataFactory: ODataServiceFactory,
	            private http: HttpClient) {
;
	}

  getAllJiraProjectsBySettingId(id: number): Observable<any[]>{
    return this.http.get<any[]>(this.constantService.jiraProject + `/GetAllJiraProjectsBySettingId?id=${id}`).pipe(
      map(response => {
        return response;
      })
    )
  }

  getNotAssignedProjects(id: number, event: TableLazyLoadEvent, filterStr = ''): 
  Observable<PagedResult<NotAssignedJiraProject>> {
    let odata = this.odataFactory.CreateService<NotAssignedJiraProject>(`JiraProject/GetUnAssignJiraProject(id=${id})`);
    let filters: string[] = []

    let query = odata
      .Query()
      .Skip(event.first)
      .Top(event.rows);

    if (event.sortField) {
      query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    } else {
      query.OrderBy('name' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    }
    if(filterStr){
      const val = filterStr.trim().toLowerCase();
      filters.push('(contains(tolower(name),\'' + val + '\') or contains(tolower(key),\'' + val + '\'))');
    }

    query.Filter(filters.join('and'));

    return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new NotAssignedJiraProject(x));
			return res;
		}));
  }

  getAssignedProjects(id: number, event: TableLazyLoadEvent, filterStr = ''): 
    Observable<PagedResult<AssignedJiraProject>> {
    let odata = this.odataFactory.CreateService<AssignedJiraProject>(`JiraProject/GetAssignJiraProject(id=${id})`);
    let filters: string[] = []

    let query = odata
      .Query()
      .Skip(event.first)
      .Top(event.rows);

    if (event.sortField) {
      query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
    } else {
      query.OrderBy('projectName' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
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
