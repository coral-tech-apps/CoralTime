
import { Component, Input, OnInit, ViewChild} from '@angular/core';
import { NotificationService } from '../../../core/notification.service';
import { AclService } from '../../../core/auth/acl.service';
import { AuthService } from '../../../core/auth/auth.service';
import { User } from '../../../models/user';
import { debounceTime, Subject, switchMap } from 'rxjs';
import { JiraSetting } from 'src/app/models/jira-setting';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { PagedResult } from 'src/app/services/odata';
import { ROWS_ON_PAGE } from 'src/app/core/constant.service';
import { Table } from 'primeng/table';

@Component({
    selector: 'ct-jira-members',
    templateUrl: 'jira-members.component.html',
    standalone: false
})

export class JiraUsersComponent implements OnInit{
  @Input() jiraSetting: JiraSetting;
  @ViewChild('assignedTable') assignTable: Table;
  @ViewChild('notAssignTable') notAssignTable: Table;

  filterStr: string = '';
  updatingGrid: boolean = false;
  isAllProjects: boolean = false;
  pagedResult: PagedResult<User>;

  assignedUsersPagedResult: PagedResult<User>;
  updatingAssignedUsersGrid: boolean = false;
	isAllAssignedUsers: boolean = false;
  private assignedUsersSubject = new Subject<any>();
  private assignedUsersLastEvent: any;

	notAssignedUsersPagedResult: PagedResult<User>;
	updatingNotAssignedUsersGrid: boolean = false;
	isAllNotAssignedUsers: boolean = false;
  private notAssignedUsersSubject = new Subject<any>();
  private notAssignedUsersLastEvent: any;

  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,){

}

  ngOnInit(): void {
    this.loadAssignedUsers();
    this.loadnUnAssignedUsers();
  }

  //Assigned users

  loadAssignedUsers(): void{
    this.assignedUsersSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraSettingService.getAssignedUsers(this.jiraSetting.id,  this.assignedUsersLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<User>) => {
      this.assignedUsersPagedResult = result;
      this.checkIsAllAssignedUsers();
    })
  }

  updateAssignedUsers(event = null, updatePage?: boolean): void {
    if (event) {
      this.assignedUsersLastEvent = event;
    }
    if (updatePage) {
      this.updatingAssignedUsersGrid = updatePage;
      this.assignedUsersLastEvent.first = 0;
    }
    if (event || updatePage) {
      this.isAllAssignedUsers = false;
      this.assignedUsersPagedResult = null;
    }
    this.assignedUsersLastEvent.rows = ROWS_ON_PAGE;
    if (!updatePage && this.isAllAssignedUsers) {
      return;
    }

    this.assignedUsersSubject.next({
      event,
      filterStr: this.filterStr
    });
  }

  private checkIsAllAssignedUsers(): void {
    if (this.assignedUsersPagedResult && this.assignedUsersPagedResult.data.length >= this.assignedUsersPagedResult.count) {
      this.isAllAssignedUsers = true;
    }
  }

  //UnAssignedUsers

  loadnUnAssignedUsers(): void{
    this.notAssignedUsersSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraSettingService.getNotAssignedUsers(this.jiraSetting.id, this.notAssignedUsersLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<User>) => {
      this.notAssignedUsersPagedResult = result;
      this.checkIsAllUnassignedUsers();
    })
  }

  updateNotAssignedUsers(event = null, updatePage?: boolean): void {
    if (event) {
      this.notAssignedUsersLastEvent = event;
    }
    if (updatePage) {
      this.updatingNotAssignedUsersGrid = updatePage;
      this.notAssignedUsersLastEvent.first = 0;
    }
    if (event || updatePage) {
      this.isAllNotAssignedUsers = false;
      this.notAssignedUsersPagedResult = null;
    }
    this.notAssignedUsersLastEvent.rows = ROWS_ON_PAGE;
    if (!updatePage && this.isAllNotAssignedUsers) {
      return;
    }

    this.notAssignedUsersSubject.next({
      event,
      filterStr: this.filterStr
    });
  }

  private checkIsAllUnassignedUsers(): void {
    if (this.notAssignedUsersPagedResult && this.notAssignedUsersPagedResult.data.length >= this.notAssignedUsersPagedResult.count) {
      this.isAllNotAssignedUsers = true;
    }
  }

  //Integration

  addIntegrationToUser(userId: number, target: HTMLElement): void{
    target.classList.add('ct-loading');
    this.jiraSettingService.assignUserToIntegration(userId, this.jiraSetting.id).subscribe(result => {
      this.updateAssignedUsers(null, true);
      this.updateNotAssignedUsers(null, true);
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
      this.updateAssignedUsers(null, true);
      this.updateNotAssignedUsers(null, true);
      this.notificationService.success('User was removed from integration.');
    },
    ()=>{
      this.notificationService.success('Error removed user from integration.');
    });
  }

  // Filtering

  onGlobalFilter(value: string){
    this.assignTable.filterGlobal(value, 'contains');
    if(this.notAssignTable){
     this.notAssignTable.filterGlobal(value, 'contains');
    }
  }
}
