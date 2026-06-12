
import {of as observableOf,  Observable } from 'rxjs';

import {mergeMap, map} from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PagedResult, ODataServiceFactory } from './odata';
import { ODataConfiguration } from './odata/config';
import { Project } from '../models/project';
import { TableLazyLoadEvent } from 'primeng/table';

@Injectable()
export class ProjectsService {
	private readonly baseUrl = '/api/v1/Projects';

	constructor(private http: HttpClient,
	            private odataFactory: ODataServiceFactory,
	            private odataConfig: ODataConfiguration) {
	}

	create(project: Project): Observable<Project> {
		return this.http.post<Project>(this.baseUrl, project);
	}

	update(project: Project): Observable<Project> {
		return this.http.put<Project>(`${this.baseUrl}/${project.id}`, project);
	}

	delete(id: number): Observable<any> {
		return this.http.delete(`${this.baseUrl}/${id}`);
	}

	getProjects(): Observable<Project[]> {
		const odata = this.odataFactory.CreateService<Project>('Projects/GetTimeTrackerAllProjects()');

		const filters = [];
		const query = odata
			.Query();

		query.OrderBy('name asc');
		filters.push('isActive eq true');
		query.Filter(filters.join(' and '));

		return query.Exec().pipe(map(res => res.map((x: any) => new Project(x))));
	}

	getProjectByName(name: string): Observable<Project> {
		name = name.trim().toLowerCase();
		if (!name) {
			throw new Error('Please, specify projects name');
		}

		const odata = this.odataFactory.CreateService<Project>('ProjectsNames/GetAllProjectNames()');

		const query = odata
			.Query()
			.Top(1);

		query.Filter('tolower(name) eq \'' + name + '\'');

		return query.Exec().pipe(
			mergeMap(result => {
				const project = result[0] ? new Project(result[0]) : null;
				return observableOf(project);
			}));
	}

	getManagerProjectsCount(): Observable<number> {
		const odata = this.odataFactory.CreateService<Project>('ManagerProjects/GetManageProjectsOfManager()');

		return odata.Query().ExecWithCount().pipe(map(res => res.count));
	}

	getManagerProjectsWithCount(event: TableLazyLoadEvent, filterStr = '', isActive: boolean = true): Observable<PagedResult<Project>> {
		const odata = this.odataFactory.CreateService<Project>('ManagerProjects/GetManageProjectsOfManager()');

		const filters = [];
		const query = odata
			.Query()
			.Top(event.rows)
			.Skip(event.first);

		if (event.sortField) {
			query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		} else {
			query.OrderBy('name' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		}
		if (filterStr) {
			filters.push('contains(tolower(name),\'' + filterStr.trim().toLowerCase() + '\')');
		}

		filters.push('isActive eq ' + isActive);
		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new Project(x));
			return res;
		}));
	}

	//  CLIENTS

	getClientProjects(event: TableLazyLoadEvent, filterStr = '', isActive: boolean = true, clientId: number = null): Observable<PagedResult<Project>> {
		const odata = this.odataFactory.CreateService<Project>('Projects/GetTimeTrackerAllProjects()');

		const filters = [];
		const query = odata
			.Query()
			.Top(event.rows)
			.Skip(event.first);

		if (event.sortField) {
			query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		} else {
			query.OrderBy('name' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		}
		if (filterStr) {
			filters.push('contains(tolower(name),\'' + filterStr.trim().toLowerCase() + '\')');
		}

		filters.push('clientId eq ' + clientId);
		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new Project(x));
			return res;
		}));
	}
}
