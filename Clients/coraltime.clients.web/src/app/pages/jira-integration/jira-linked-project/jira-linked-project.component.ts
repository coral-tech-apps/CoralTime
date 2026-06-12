import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { JiraMemberSetting } from './../../../models/jira-member-setting';
import { Component, Input, OnInit, ViewChild} from '@angular/core';
import { NotificationService } from '../../../core/notification.service';
import { AuthService } from '../../../core/auth/auth.service';
import { debounceTime, Subject, switchMap, tap } from 'rxjs';
import { JiraSetting } from 'src/app/models/jira-setting';
import { HttpClient } from '@angular/common/http';
import { PagedResult } from 'src/app/services/odata';
import { ROWS_ON_PAGE } from 'src/app/core/constant.service';
import { Table, TableLazyLoadEvent } from 'primeng/table';
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
  jiraMemberSetting: JiraMemberSetting;
  isAvaliableLoadProjects: boolean;

  assignedJiraProjects: PagedResult<AssignedJiraProject>;
  firstLoadingAssignedJiraProject: boolean = true;
  updatingAssignedJiraProject: boolean = false;
  isAssingedProjects: boolean = false;
  private assignedProjectsSubject = new Subject<any>();
  public assignedProjectsLastEvent: TableLazyLoadEvent = {
    first: 0,
    rows: ROWS_ON_PAGE
  };

  notAssignedJiraProjects: PagedResult<NotAssignedJiraProject>;
  firstLoadingNotAssignedJiraProject: boolean = true;
  updatingNotAssignedJiraProject: boolean = false;
  isNotAssingedProjects: boolean = false;
  private notAssignedProjectsSubject = new Subject<any>();
  public notAssignedProjectsLastEvent: TableLazyLoadEvent = {
    first: 0,
    rows: ROWS_ON_PAGE
  };

  private projectsLastEvent: TableLazyLoadEvent;

  constructor(private http: HttpClient,
    public authService: AuthService,
    private projectService: ProjectsService,
    private jiraProjectService: JiraProjectService,
    private notificationService: NotificationService,
    private jiraSettingService: JiraSettingService){

}

  ngOnInit(): void {
    this.getJiraMemberSetting();
    this.loadProjects();
    this.loadUnAssignedProjects();
    this.loadAssignedProjects();
  }

  loadNewProjects(): void{
    this.isNotAssingedProjects = false;

    if (!this.jiraMemberSetting.apiTokenStatus) {
      this.notificationService.danger("Error: API token is missing. Please fill it in Preferences.");
      return;
    }
    if (!this.jiraMemberSetting.userEmail) {
      this.notificationService.danger("Error: Email is missing. Please fill it in Preferences.");
      return;
    }
    if (!this.jiraMemberSetting.isEnableConntection) {
      this.notificationService.danger("Error: Jira connection is not verified. Please check it in Preferences.");
      return;
    }

    this.jiraProjectService.loadJiraProjects(this.jiraSetting.id).subscribe( () => {
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
    this.assignedProjectsSubject.pipe(debounceTime(500),
    tap(() => {
      this.updatingAssignedJiraProject = true;
    }),
    switchMap(() => {
      return this.jiraProjectService.getAssignedProjects(this.jiraSetting.id, this.assignedProjectsLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<AssignedJiraProject>) => {
      if (!this.assignedJiraProjects) {
        this.firstLoadingAssignedJiraProject = false;
        this.assignedJiraProjects = result;
      } else {
        this.assignedJiraProjects.data = this.assignedJiraProjects.data.concat(result.data);
      }
      this.assignedProjectsLastEvent = {...this.assignedProjectsLastEvent, 
        first: (this.assignedJiraProjects?.data.length ?? 0) + ROWS_ON_PAGE};
      this.isAssingedProjects = result.count <= this.assignedJiraProjects.data.length;

      this.updatingAssignedJiraProject = false;
    })
  }

  updateAssignedProjects(event: TableLazyLoadEvent | null = null, updatePage?: boolean): void {
    if (event) {
      this.assignedProjectsLastEvent = event;
    }
    if (event || updatePage) {
      this.isAssingedProjects = false;
    }

    if (!updatePage && this.isAssingedProjects) {
      return;
    }

    this.assignedProjectsSubject.next({
      event: this.assignedProjectsLastEvent,
      filterStr: this.filterStr
    });
  }

  //NotAssigned Projects

  loadUnAssignedProjects(): void {
    this.notAssignedProjectsSubject.pipe(debounceTime(500),
    tap(() => {
      this.updatingNotAssignedJiraProject = true;
    }),
    switchMap(() => {
      return this.jiraProjectService.getNotAssignedProjects(this.jiraSetting.id, this.notAssignedProjectsLastEvent, this.filterStr)
    }))
    .subscribe((result: PagedResult<NotAssignedJiraProject>) => {
      if (!this.notAssignedJiraProjects) {
        this.firstLoadingNotAssignedJiraProject = false;
        this.notAssignedJiraProjects = result;
      } else {
        this.notAssignedJiraProjects.data = this.notAssignedJiraProjects.data.concat(result.data);
      }
      
      this.notAssignedProjectsLastEvent = {...this.notAssignedProjectsLastEvent, 
        first: (this.notAssignedJiraProjects?.data.length ?? 0) + ROWS_ON_PAGE};
      this.isNotAssingedProjects = result.count <= this.notAssignedJiraProjects.data.length;

      this.updatingNotAssignedJiraProject = false;
    })
  }

  updateNotAssignedProjects(event: TableLazyLoadEvent | null = null, updatePage?: boolean): void {
    if (event) {
      this.notAssignedProjectsLastEvent = event;
    }
    if (event || updatePage) {
      this.isNotAssingedProjects = false;
    }
    if (!updatePage && this.isNotAssingedProjects) {
      return;
    }

    this.notAssignedProjectsSubject.next({
      event: this.notAssignedProjectsLastEvent,
      filterStr: this.filterStr
    });
  }


  onGlobalFilter(value: string){
    this.assignTable.filterGlobal(value, 'contains');
    if(this.notAssignTable){
      this.notAssignTable.filterGlobal(value, 'contains');
    }
  }

  onEndScrollAssigned(): void {
    if (!this.isAssingedProjects) {
      this.updateAssignedProjects(this.assignedProjectsLastEvent, true);
    }
  }
  
  onEndScrollNotAssigned(): void {
    if (!this.isNotAssingedProjects) {
      this.updateNotAssignedProjects(this.notAssignedProjectsLastEvent, true);
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

  getJiraMemberSetting(): void{
    this.jiraSettingService.getJiraMemberSetting(this.jiraSetting.id).subscribe((result: JiraMemberSetting) => {
      if(result == undefined){
        this.isAvaliableLoadProjects = false;
      }else{
        this.isAvaliableLoadProjects = true;
      }
      this.jiraMemberSetting = result;
    });
  }

}
