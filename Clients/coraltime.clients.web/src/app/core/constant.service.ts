import { Injectable } from '@angular/core';
import { TableLazyLoadEvent } from 'primeng/table';
import { PagedResult } from '../services/odata';

export const ROWS_ON_PAGE = 100;
export const EMAIL_PATTERN = /^(([^<>()\[\]\.,;:\s@\"]+(\.[^<>()\[\]\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()\.,;\s@\"]+\.{0,1})+[^<>()\.,;:\s@\"]{2,})$/;
export const URL_PATTERN = /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)$/i;
export const OUTPUT_DATE_FORMAT = 'YYYY-MM-DD';

export const DEFAULT_TABLE_LOAD_EVENT: TableLazyLoadEvent = {
	first: 0,
	rows: ROWS_ON_PAGE,
	sortOrder: 1
};

export const createDefaultPageResult = <T>(): PagedResult<T> => ({
	count: 0,
	data: []
});

@Injectable()
export class ConstantService {
	apiBaseUrl = '/api/v1';
	jiraApi = '/api/v1/Jira';
	jiraProject = this.apiBaseUrl + '/JiraProject';
  jiraWorklog = this.apiBaseUrl + '/JiraWorklog';
	adminApi = this.apiBaseUrl + '/Admin/';
	profileApi: string = this.apiBaseUrl + '/Profile';
	reportsApi: string = this.apiBaseUrl + '/Reports';
	timeEntriesApi: string = this.apiBaseUrl + '/TimeEntries/';
}
