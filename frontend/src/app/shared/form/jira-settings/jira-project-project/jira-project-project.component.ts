import { Subject } from 'rxjs';
import { JiraSetting } from 'src/app/models/jira-setting';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Component, Input, OnInit } from '@angular/core';
import { User } from 'src/app/models/user';
import { AuthService } from 'src/app/core/auth/auth.service';
import { NotificationService } from 'src/app/core/notification.service';
import { JiraProjectService } from 'src/app/services/jira-project.service';
import { JiraMemberSetting } from 'src/app/models/jira-member-setting';


@Component({
    selector: 'ct-jira-project',
    templateUrl: 'jira-project-project.component.html',
    standalone: false
})

export class JiraProjectProjectComponent implements OnInit{
  @Input() jiraSetting: JiraMemberSetting;

  jiraSettingId: number;

  AddssignedJiraProjects: User[];
  updatingAssignedJiraProject: boolean = false;
  isAssingedProjects: boolean = false;

  notAddssignedJiraProjects: User[]
  updatingNotAssignedJiraProject: boolean = false;
  isNotAssingedProjects: boolean = false;
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

  loadAllProjects(): void{
    this.jiraProjectService.loadJiraProjects(this.jiraSetting.domain, this.jiraSetting.userEmail, this.jiraSetting.apiToken).subscribe();
  }

  loadAssignedProjects(): void{
    this.jiraProjectService.getAssignedProjects(this.jiraSettingId).subscribe(result => {
      this.AddssignedJiraProjects = [];
      this.AddssignedJiraProjects = result;
      this.isAssingedProjects = true;
    })
  }

  loadUnAssignedProjects(): void{
    this.jiraProjectService.getNotAssignedProjects(this.jiraSettingId).subscribe(result => {
      this.notAddssignedJiraProjects = [];
      this.notAddssignedJiraProjects = result;
      this.isNotAssingedProjects = true;
    })
  }
}

