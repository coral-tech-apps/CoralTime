
import {of as observableOf,  Observable } from 'rxjs';

import {mergeMap, map} from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PagedResult, ODataServiceFactory, ODataService } from './odata';
import { Client } from '../models/client';

@Injectable()
export class ClientsService {
	private readonly baseUrl = '/api/v1/Clients';

	constructor(private http: HttpClient,
	            private odataFactory: ODataServiceFactory) {
	}

	create(client: Client): Observable<Client> {
		return this.http.post<Client>(this.baseUrl, client);
	}

	update(client: Client): Observable<Client> {
		return this.http.put<Client>(`${this.baseUrl}/${client.id}`, client);
	}

	delete(id: number): Observable<any> {
		return this.http.delete(`${this.baseUrl}/${id}`);
	}

	getClients(): Observable<Client[]> {
		const odata = this.odataFactory.CreateService<Client>('Clients/GetAllClients()');
		
		const filters = [];
		const query = odata
			.Query();

		query.OrderBy('name asc');
		filters.push('isActive eq true');
		query.Filter(filters.join(' and '));

		return query.Exec().pipe(map(res => res.map((x: Object) => new Client(x))));
	}

	getClientsWithCount(event, filterStr = '', isActive: boolean = true): Observable<PagedResult<Client>> {
		const odata = this.odataFactory.CreateService<Client>('Clients/GetAllClients()');
		
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
			res.data = res.data.map((x: Object) => new Client(x));
			return res;
		}));
	}

	getClientByName(name: string): Observable<Client> {
		name = name.trim().toLowerCase();
		if (!name) {
			throw new Error('Please, specify client name');
		}

		const odata = this.odataFactory.CreateService<Client>('Clients/GetAllClients()');
		const query = odata
			.Query()
			.Top(1);

		query.Filter('tolower(name) eq \'' + name + '\'');

		return query.Exec().pipe(
			mergeMap(result => {
				let client = result[0] ? new Client(result[0]) : null;
				return observableOf(client);
			}));
	}
}
