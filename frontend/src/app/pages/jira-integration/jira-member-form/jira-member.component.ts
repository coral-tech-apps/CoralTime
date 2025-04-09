
import {finalize, switchMap, debounceTime} from 'rxjs/operators';
import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Project } from '../../../models/project';
import { UserProject } from '../../../models/user-project';
import { UsersService } from '../../../services/users.service';
import { NotificationService } from '../../../core/notification.service';
import { AclService } from '../../../core/auth/acl.service';
import { AuthUser } from '../../../core/auth/auth-user';
import { AuthService } from '../../../core/auth/auth.service';
import { ProjectRolesService } from '../../../services/project-roles.service';
import { SettingsService } from '../../../services/settings.service';
import { ProjectRole } from '../../../models/project-role';
import { User } from '../../../models/user';
import { PagedResult } from '../../../services/odata';
import { Subject } from 'rxjs';
import { ArrayUtils } from '../../../core/object-utils';
import { ROWS_ON_PAGE } from '../../../core/constant.service';
import { Table } from 'primeng/table';
import { JiraSetting } from 'src/app/models/jira-setting';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';

@Component({
    selector: 'ct-jira-members',
    templateUrl: 'jira-members.component.html',
    standalone: false
})

export class JiraUsersComponent implements OnInit{
	@Input() jiraSetting: JiraSetting;

  //assignedUsersPagedResult: PagedResult<UserProject>;
	AssignedUsersPagedResult: User[];
  updatingAssignedUsersGrid: boolean = false;
	isAllAssignedUsers: boolean = false;
	private assignedUsersSubject = new Subject<any>();
	private assignedUsersLastEvent: any;

	//notAssignedUsersPagedResult: PagedResult<User>;
  notAssignedUsersPagedResult: User[]
	updatingNotAssignedUsersGrid: boolean = false;
	isAllNotAssignedUsers: boolean = false;
	private notAssignedUsersLastEvent: any;
	private notAssignedUsersSubject = new Subject<any>();

  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,
    private dialog: MatDialog,){

}

  ngOnInit(): void {
    this.loadAssignedUsers();
    this.loadnUnAssignedUsers();
  }

  loadAssignedUsers(): void{
    this.jiraSettingService.getAssignedUsers(this.jiraSetting.id).subscribe(result => {
      this.AssignedUsersPagedResult = result;
    })
  }

  loadnUnAssignedUsers(): void{
    this.jiraSettingService.getNotAssignedUsers(this.jiraSetting.id).subscribe(result => {
      this.notAssignedUsersPagedResult = result;
    })
  }

  addIntegrationToUser(userId: number): void{
    this.jiraSettingService.assignUserToIntegration(userId, this.jiraSetting.id).subscribe(result => {

    });
  }

  removeIntegrationFromUser(userId: number): void{
    this.jiraSettingService.removeIntegrationFromUser(userId, this.jiraSetting.id).subscribe();
  }
}
