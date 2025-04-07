import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ODataServiceFactory, ODataService } from './odata';
import { JiraSetting } from '../models/jira-setting';
import { ConstantService } from '../core/constant.service';

@Injectable()
export class JiraSettingService {
	readonly odata: ODataService<JiraSetting>;
  authService: any;

	constructor(private constantService: ConstantService,
	            private http: HttpClient,
	            private odataFactory: ODataServiceFactory) {
		this.odata = this.odataFactory.CreateService<JiraSetting>('JiraSetting');
	}

  isEnableJira(): boolean{
    return false;
  }

  loadSettingsTable(id: number): any{
    return [];
  }

  createNewSetting(jiraSetting: JiraSetting): Observable<any>{
    return this.http.post('http://localhost:4200/api/v1/Jira', jiraSetting)
  }

  updateSetting(jiraSetting: JiraSetting, id: string): Observable<any>{
    return this.http.patch(`http://localhost:4200/api/v1/Jira?id=${id}`, jiraSetting)
  }
}
