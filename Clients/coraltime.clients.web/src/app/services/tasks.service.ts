
import {of as observableOf,  Observable } from 'rxjs';

import {mergeMap, map} from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PagedResult, ODataServiceFactory, ODataService } from './odata';
import { Task } from '../models/task';

@Injectable()
export class TasksService {
	readonly odata: ODataService<Task>;
	private readonly baseUrl = '/api/v1/Tasks';

	constructor(private http: HttpClient,
	            private odataFactory: ODataServiceFactory) {
		this.odata = this.odataFactory.CreateService<Task>('Tasks/GetAllTasks()');
	}

	create(task: Task): Observable<Task> {
		return this.http.post<Task>(this.baseUrl, task);
	}

	update(task: Task): Observable<Task> {
		return this.http.put<Task>(`${this.baseUrl}/${task.id}`, task);
	}

	delete(id: number): Observable<any> {
		return this.http.delete(`${this.baseUrl}/${id}`);
	}

	getActiveTasks(projectId?: number): Observable<PagedResult<Task>> {
		let filters = [];
		let query = this.odata
			.Query();

		query.OrderBy('name asc');
		if (projectId) {
			filters.push('(projectId eq ' + projectId + ' or projectId eq null)');
		} else {
			filters.push('projectId eq null');
		}
		filters.push('isActive eq true');
		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new Task(x));
			return res;
		}));
	}

	getProjectTasks(event, filterStr = '', projectId: number): Observable<PagedResult<Task>> {
		let filters = [];
		let query = this.odata
			.Query()
			.Top(event.rows)
			.Skip(event.first);

		if (event.sortField) {
			query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		} else {
			query.OrderBy('name asc');
		}

		if (filterStr) {
			filters.push('contains(tolower(name),\'' + filterStr.trim().toLowerCase() + '\')');
		}

		filters.push('isActive eq true');
		filters.push('(projectId eq ' + projectId + ' or projectId eq null)');
		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new Task(x));
			return res;
		}));
	}

	getTaskByName(name: string): Observable<Task> {
		name = name.trim().toLowerCase();
		if (!name) {
			throw new Error('Please, specify task name');
		}

		let query = this.odata
			.Query()
			.Top(1);

		query.Filter('tolower(name) eq \'' + name + '\' and projectId eq null');

		return query.Exec().pipe(
			mergeMap(result => {
				let task = result[0] ? new Task(result[0]) : null;
				return observableOf(task);
			}));
	}

	getTasksWithCount(event, filterStr = '', isActive: boolean = true): Observable<PagedResult<Task>> {
		let filters = [];
		let query = this.odata
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
		filters.push('projectId eq null');
		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new Task(x));
			return res;
		}));
	}

	toggleActive(task: Task): Observable<any> {
		task.isActive = !task.isActive;

		return this.http.patch(`${this.baseUrl}/${task.id}`, {
			isActive: task.isActive,
            name: task.name,
            projectId: task.projectId,
			color: task.color,
			description: task.description
		});
	}
}
