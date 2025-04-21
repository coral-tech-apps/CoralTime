
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
import { Project } from 'src/app/models/project';
import { ProjectsService } from 'src/app/services/projects.service';
import { AssignedJiraProject } from 'src/app/models/assigned-jira-project';
import { NotAssignedJiraProject } from 'src/app/models/not-assigned-jira-project';
import { JiraProjectService } from 'src/app/services/jira-project.service';

@Component({
    selector: 'ct-jira-linked-project',
    templateUrl: 'jira-linked-project.component.html',
    standalone: false
})

export class JiraLinkedProjectComponent implements OnInit{
  @Input() jiraSetting: JiraSetting;
  @ViewChild('assignedTable') assignTable: Table;
  @ViewChild('notAssignTable') notAssignTable: Table;

  filterStr: string = '';
  projects: Project[];

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

  private projectsLastEvent: any;

  constructor(private http: HttpClient,
    public authService: AuthService,
    private projectService: ProjectsService,
    private jiraProjectService: JiraProjectService,
    private notificationService: NotificationService,){

}

  ngOnInit(): void {
   this.loadProjects();
   this.loadUnAssignedProjects();
   this.loadAssignedProjects();
  }

  loadProjects(): void{
    if(!this.projectsLastEvent){
      this.projectsLastEvent = {
        first: 0,
        rows: ROWS_ON_PAGE
      }
    }
    this.projectService.getClientProjects(this.projectsLastEvent, this.filterStr, true, this.jiraSetting.clientId).subscribe(result => {
      this.projects = result.data;
    })
  }

  // AssignedProject

  loadAssignedProjects(): void{
    this.assignedProjectsSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraProjectService.getAssignedProjects(this.jiraSetting.id, this.assignedProjectsLastEvent, this.filterStr)
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

  //NotAssigned Projects

  loadUnAssignedProjects(): void{
    this.notAssignedProjectsSubject.pipe(debounceTime(500),switchMap(() => {
      return this.jiraProjectService.getNotAssignedProjects(this.jiraSetting.id, this.notAssignedProjectsLastEvent, this.filterStr)
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


  onGlobalFilter(value: string){
    this.assignTable.filterGlobal(value, 'contains');
    if(this.notAssignTable){
     this.notAssignTable.filterGlobal(value, 'contains');
    }
  }

  onSelectionChange(event: any, jiraProject: NotAssignedJiraProject):void{
    this.jiraProjectService.linkProjects(event.value.id, jiraProject.id).subscribe(() => {
      this.updateAssignedProjects(null, true);
      this.updateNotAssignedProjects(null, true);
      this.notificationService.success("Project linked successfully");
    },
    () => {
      this.updateAssignedProjects(null, true);
      this.updateNotAssignedProjects(null, true);
      this.notificationService.danger("Error occured while linking projects");
    });
  }

  unLink(assignedJiraProject: AssignedJiraProject): void{
    this.jiraProjectService.removeProjectJiraLink(assignedJiraProject.id).subscribe(() => {
      this.updateAssignedProjects(null, true);
      this.updateNotAssignedProjects(null, true);
      this.notificationService.success("Project link removed successfully");
    },
    () => {
      this.updateAssignedProjects(null, true);
      this.updateNotAssignedProjects(null, true);
      this.notificationService.danger("Error occured while unlinking projects");
    }
  )
  }
}
