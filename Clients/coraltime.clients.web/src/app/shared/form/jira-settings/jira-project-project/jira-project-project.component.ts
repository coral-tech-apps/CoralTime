import { debounceTime, Subject, switchMap, tap } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';
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
    private jiraProjectService: JiraProjectService,){

}

  ngOnInit(): void {
    this.loadAssignedProjects();
    this.loadUnAssignedProjects();

  }

  //Assigned Projects

  loadAssignedProjects(): void{
    this.assignedProjectsSubject.pipe(debounceTime(500),
      tap(() => { this.updatingAssignedJiraProject = true; }),
      switchMap(() => {
      return this.jiraProjectService.getAssignedProjects(this.jiraSettingId, this.assignedProjectsLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<AssignedJiraProject>) => {
      if (!this.assignedJiraProjects || !this.assignedProjectsLastEvent.first || this.updatingAssignedJiraProject) {
        this.assignedJiraProjects = result;
      } else {
        this.assignedJiraProjects.data = this.assignedJiraProjects.data.concat(result.data);
      }

      this.assignedProjectsLastEvent.first = this.assignedJiraProjects.data.length;
      this.updatingAssignedJiraProject = false;
      this.checkIsAllAssignedProjects();
    })
  }

  onAssignedProjectsEndScroll(): void {
    if (!this.isAssingedProjects) {
      this.updateAssignedProjects();
    }
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
    this.notAssignedProjectsSubject.pipe(debounceTime(500),
      tap(() => { this.updatingNotAssignedJiraProject = true; }),
      switchMap(() => {
      return this.jiraProjectService.getNotAssignedProjects(this.jiraSettingId, this.notAssignedProjectsLastEvent, this.filterStr)
    }),)
    .subscribe((result: PagedResult<NotAssignedJiraProject>) => {
      if (!this.notAssignedJiraProjects || !this.notAssignedProjectsLastEvent.first || this.updatingNotAssignedJiraProject) {
        this.notAssignedJiraProjects = result;
      } else {
        this.notAssignedJiraProjects.data = this.notAssignedJiraProjects.data.concat(result.data);
      }

      this.notAssignedProjectsLastEvent.first = this.notAssignedJiraProjects.data.length;
      this.updatingNotAssignedJiraProject = false;
      this.checkIsAllNotAssignedProjects();
    })
  }

  onNotAssignedProjectsEndScroll(): void {
    if (!this.isNotAssingedProjects) {
      this.updateNotAssignedProjects();
    }
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

  // Filtering

  onGlobalFilter(value: string){
    this.assignTable.filterGlobal(value, 'contains');
    if(this.notAssignTable){
     this.notAssignTable.filterGlobal(value, 'contains');
    }
  }

  private checkIsAllAssignedProjects(): void {
    if (this.assignedJiraProjects && this.assignedJiraProjects.data.length >= this.assignedJiraProjects.count) {
      this.isAssingedProjects = true;
    }
  }

  private checkIsAllNotAssignedProjects(): void {
    if (this.notAssignedJiraProjects && this.notAssignedJiraProjects.data.length >= this.notAssignedJiraProjects.count) {
      this.isNotAssingedProjects = true;
    }
  }
}

