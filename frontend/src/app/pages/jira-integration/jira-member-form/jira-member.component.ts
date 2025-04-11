
import { Component, Input, OnInit} from '@angular/core';
import { NotificationService } from '../../../core/notification.service';
import { AclService } from '../../../core/auth/acl.service';
import { AuthService } from '../../../core/auth/auth.service';
import { User } from '../../../models/user';
import { Subject } from 'rxjs';
import { JiraSetting } from 'src/app/models/jira-setting';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { PagedResult } from 'src/app/services/odata';

@Component({
    selector: 'ct-jira-members',
    templateUrl: 'jira-members.component.html',
    standalone: false
})

export class JiraUsersComponent implements OnInit{
  filterStr: string = '';

	@Input() jiraSetting: JiraSetting;

  //assignedUsersPagedResult: PagedResult<UserProject>;
	AssignedUsersPagedResult: User[];
  updatingAssignedUsersGrid: boolean = false;
	isAllAssignedUsers: boolean = false;

	//notAssignedUsersPagedResult: PagedResult<User>;
  notAssignedUsersPagedResult: User[]
	updatingNotAssignedUsersGrid: boolean = false;
	isAllNotAssignedUsers: boolean = false;

  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,){

}

  ngOnInit(): void {
    this.loadAssignedUsers(this.filterStr);
    this.loadnUnAssignedUsers(this.filterStr);
  }

  loadAssignedUsers(filterStr: string): void{
    this.isAllAssignedUsers = false;
    this.jiraSettingService.getAssignedUsers(this.jiraSetting.id, filterStr).subscribe((result: PagedResult<User>) => {
      this.AssignedUsersPagedResult = result.data;
      this.isAllAssignedUsers = true;
    })
  }

  loadnUnAssignedUsers(filterStr: string): void{
    this.isAllNotAssignedUsers = false;
    this.jiraSettingService.getNotAssignedUsers(this.jiraSetting.id, filterStr).subscribe((result: PagedResult<User>) => {
      this.notAssignedUsersPagedResult = result.data;
      this.isAllNotAssignedUsers = true;
    })
  }

  addIntegrationToUser(userId: number, target: HTMLElement): void{
    target.classList.add('ct-loading');
    this.jiraSettingService.assignUserToIntegration(userId, this.jiraSetting.id).subscribe(result => {
      console.log(result);
      this.loadAssignedUsers(this.filterStr);
      this.loadnUnAssignedUsers(this.filterStr);
      this.notificationService.success('User was added to integration.');
    },
    (error)=>{
      this.notificationService.success('Error added user to integration.');
      target.classList.remove('ct-loading');
    });
  }

  removeIntegrationFromUser(userId: number, target: HTMLElement): void{
    target.classList.add('ct-loading');
    this.jiraSettingService.removeIntegrationFromUser(userId, this.jiraSetting.id).subscribe(result =>{
      console.log(result);
      this.loadAssignedUsers(this.filterStr);
      this.loadnUnAssignedUsers(this.filterStr);
      this.notificationService.success('User was removed from integration.');
    },
    ()=>{
      this.notificationService.success('Error removed user from integration.');
    });
  }

  filterTable(value: string): void{
    this.loadAssignedUsers(value);
    this.loadnUnAssignedUsers(value);
  }
}
