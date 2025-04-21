import { debounceTime, Subject, switchMap } from 'rxjs';
import { JiraSetting } from 'src/app/models/jira-setting';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/models/user';
import { AuthService } from 'src/app/core/auth/auth.service';
import { NotificationService } from 'src/app/core/notification.service';
import { JiraProjectService } from 'src/app/services/jira-project.service';
import { JiraMemberSetting } from 'src/app/models/jira-member-setting';
import { ROWS_ON_PAGE } from 'src/app/core/constant.service';
import { Table } from 'primeng/table';
import { PagedResult } from 'src/app/services/odata';
import { NotAssignedJiraProject } from 'src/app/models/not-assigned-jira-project';
import { AssignedJiraProject } from 'src/app/models/assigned-jira-project';

@Component({
    selector: 'ct-jira-project',
    templateUrl: 'jira-project-project.component.html',
    standalone: false
})

export class JiraProjectProjectComponent implements OnInit{
  @Input() jiraSetting: JiraMemberSetting;
  @ViewChild('assignedTable') assignTable: Table;
  @ViewChild('notAssignTable') notAssignTable: Table;

  filterStr: string = '';
  jiraSettingId: number;

  assignedJiraProjects: PagedResult<AssignedJiraProject>;
  updatingAssignedJiraProject: boolean = false;
  isAssingedProjects: boolean = false;
  private assignedProjectsSubject = new Subject<any>();
  private assignedProjectsLastEvent: any;

  notAssignedJiraProjects: PagedResult<NotAssignedJiraProject>;
  updatingNotAssignedJiraProject: boolean = false;
  isNotAssingedProjects: boolean = false;
  private notAssignedProjectsSubject = new Subject<any>();
  private notAssignedProjectsLastEvent: any;


  onSubmit: any;

  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraProjectService: JiraProjectService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,){

}

  ngOnInit(): void {
    this.loadAssignedProjects();
    this.loadUnAssignedProjects();

  }

  //Assigned Projects

  loadAssignedProjects(): void{
    this.assignedProjectsSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraProjectService.getAssignedProjects(this.jiraSettingId, this.assignedProjectsLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<AssignedJiraProject>) => {
      this.assignedJiraProjects = result;
      this.isAssingedProjects = true;
    })
  }

  updateAssignedProjects(event = null, updatePage?: boolean): void {
    if (event) {
      this.assignedProjectsLastEvent = event;
    }
    if (updatePage) {
      this.updatingAssignedJiraProject = updatePage;
      this.assignedProjectsLastEvent.first = 0;
    }
    if (event || updatePage) {
      this.isAssingedProjects = false;
      this.assignedJiraProjects = null;
    }
    this.assignedProjectsLastEvent.rows = ROWS_ON_PAGE;
    if (!updatePage && this.isAssingedProjects) {
      return;
    }

    this.assignedProjectsSubject.next({
      event,
      filterStr: this.filterStr
    });
  }

  //Not Assigned Projects

  loadUnAssignedProjects(): void{
    this.notAssignedProjectsSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraProjectService.getNotAssignedProjects(this.jiraSettingId, this.notAssignedProjectsLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<NotAssignedJiraProject>) => {
      this.notAssignedJiraProjects = result;
      this.isNotAssingedProjects = true;
    })
  }

  updateNotAssignedProjects(event = null, updatePage?: boolean): void {
    if (event) {
      this.notAssignedProjectsLastEvent = event;
    }
    if (updatePage) {
      this.updatingNotAssignedJiraProject = updatePage;
      this.notAssignedProjectsLastEvent.first = 0;
    }
    if (event || updatePage) {
      this.isNotAssingedProjects = false;
      this.notAssignedJiraProjects = null;
    }
    this.notAssignedProjectsLastEvent.rows = ROWS_ON_PAGE;
    if (!updatePage && this.isNotAssingedProjects) {
      return;
    }

    this.notAssignedProjectsSubject.next({
      event,
      filterStr: this.filterStr
    });
  }

  //All projects

  loadAllProjects(): void{
    this.isNotAssingedProjects = false;
    this.jiraProjectService.loadJiraProjects(this.jiraSetting.jiraSettingId).subscribe( () => {
      this.notAssignedProjectsSubject.next({
        event,
        filterStr: this.filterStr
      });
        this.notificationService.success("Projects load succesfully");
      },
      () => {
        this.notificationService.danger("Error while loading projects");
      }
    );
  }

  // Filtering

  onGlobalFilter(value: string){
    this.assignTable.filterGlobal(value, 'contains');
    if(this.notAssignTable){
     this.notAssignTable.filterGlobal(value, 'contains');
    }
  }
}

