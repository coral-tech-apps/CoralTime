
import {of as observableOf,  Observable } from 'rxjs';

import {mergeMap, map} from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PagedResult, ODataServiceFactory, ODataService } from './odata';
import { VstsProjectConnection, VstsUser } from '../models/vsts-project-connection';
import { ConstantService } from '../core/constant.service';

@Injectable()
export class VstsIntegrationService {
	private readonly baseUrl = '/api/v1/VstsProjectIntegration';

	constructor(private constantService: ConstantService,
	            private http: HttpClient,
	            private odataFactory: ODataServiceFactory) {
	}

	getConnectionsWithCount(event, filterStr = ''): Observable<PagedResult<VstsProjectConnection>> {
		const odata = this.odataFactory.CreateService<VstsProjectConnection>('VstsProjectIntegration/GetAllVstsProjects()');

		const filters = [];
		const query = odata
			.Query()
			.Top(event.rows)
			.Skip(event.first);

		if (event.sortField) {
			query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		} else {
			query.OrderBy('projectName' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		}

		if (filterStr) {
			filters.push('contains(tolower(projectName),\'' +
				filterStr.trim().toLowerCase() + '\')' +
				' or contains(tolower(vstsProjectName),\'' +
				filterStr.trim().toLowerCase() + '\')');
		}

		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new VstsProjectConnection(x));
			return res;
		}));
	}

	getConnectionsByProjectName(name: string): Observable<VstsProjectConnection> {
		name = name.trim().toLowerCase();
		if (!name) {
			throw new Error('Please, specify project name');
		}

		const odata = this.odataFactory.CreateService<VstsProjectConnection>('VstsProjectIntegration/GetAllVstsProjects()');
		const query = odata
			.Query()
			.Top(1);

		query.Filter('tolower(vstsProjectName) eq \'' + name + '\'');

		return query.Exec().pipe(
			mergeMap(result => {
				let project = result[0] ? new VstsProjectConnection(result[0]) : null;
				return observableOf(project);
			}));
	}

	getConnectionsMembersWithCount(event, filterStr = '', connectionId: number): Observable<PagedResult<any>> {
		const odata = this.odataFactory.CreateService<VstsUser>('VstsProjectIntegration/GetMembers(id=' + connectionId + ')');

		const filters = [];
		const query = odata
			.Query()
			.Top(event.rows)
			.Skip(event.first);

		if (event.sortField) {
			query.OrderBy(event.sortField + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		} else {
			query.OrderBy('fullName' + ' ' + (event.sortOrder === 1 ? 'asc' : 'desc'));
		}

		if (filterStr) {
			filters.push('contains(tolower(fullName),\'' + filterStr.trim().toLowerCase() + '\')');
		}

		query.Filter(filters.join(' and '));

		return query.ExecWithCount().pipe(map(res => {
			res.data = res.data.map((x: Object) => new VstsUser(x));
			return res;
		}));
	}

	updateConnection(id: number, connection: VstsProjectConnection): Observable<VstsProjectConnection> {
		return this.http.put<VstsProjectConnection>(`${this.baseUrl}/${id}`, connection);
	}

	createConnection(connection: VstsProjectConnection): Observable<VstsProjectConnection> {
		return this.http.post<VstsProjectConnection>(this.baseUrl, connection);
	}

	deleteConnection(id: number): Observable<void> {
		return this.http.delete<void>(`${this.baseUrl}/${id}`);
	}

	updateVstsUsers(): Observable<any> {
		return this.http.get(this.constantService.adminApi + 'UpdateVstsUsers');
	};
}
