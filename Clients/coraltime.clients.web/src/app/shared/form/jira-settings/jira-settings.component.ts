import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { AuthService } from "src/app/core/auth/auth.service";
import { JiraSettingFormComponent } from "./form/jira-settings-form.component";
import { JiraSettingService } from "src/app/services/jira-settings.service";
import { NotificationService } from "src/app/core/notification.service";
import { JiraMemberSetting } from "src/app/models/jira-member-setting";
import { JiraProjectProjectComponent } from "./jira-project-project/jira-project-project.component";

@Component({
  selector: 'ct-jira-settings',
  templateUrl: 'jira-settings.component.html',
  standalone: false
})

export class JiraSettings implements OnInit{
showJiraTable: boolean;
tableData: JiraMemberSetting[] = [];

private dialogRef: MatDialogRef<JiraSettingFormComponent>;
private jiraProjectDialogRef: MatDialogRef<JiraProjectProjectComponent>;

  constructor(private http: HttpClient,
              public authService: AuthService,
              private jiraSettingService: JiraSettingService,
              private notificationService: NotificationService,
              private dialog: MatDialog,){

  }

  ngOnInit(){
    this.loadInitialState();
  }

  loadInitialState(){
    this.jiraSettingService.isEnableJira().subscribe(result => {
      this.showJiraTable = result;
      if(this.showJiraTable){
        this.loadJiraTable();
      }
    });
  }

  onToggle(){
    this.jiraSettingService.changeJiraStatus(this.showJiraTable).subscribe(result => {
      this.showJiraTable = result;
    })

    if(this.showJiraTable){
      this.loadJiraTable();
    }
  }

  openConnectionDialog(setting: JiraMemberSetting = null): void {
    this.dialogRef = this.dialog.open(JiraSettingFormComponent);
    this.dialogRef.componentInstance.setting = setting;
    this.dialogRef.componentInstance.onSubmit.subscribe((response) => {
      this.dialogRef.close();
      this.onSubmit(response);
    });
  }

  checkConncetion(jiraSettingId: number, settingName: string): void{
    this.jiraSettingService.getJiraUserIdStatus(jiraSettingId).subscribe(
      {
        next: (response) => {
          this.loadJiraTable();
          this.notificationService.success("Connection has been checked");
        },
        error: (err) => {
          this.notificationService.danger(`Error while checking connection ${settingName}`);
        }
      }
    )
  }

  private onSubmit(response: any): void {
		if (response.error) {
			this.notificationService.danger('Error saving jira setting.');
			return;
		}
		this.notificationService.success('Jira setting has been successfully changed.');
    this.loadInitialState();
	}

  onJiraProjectDialog(setting: JiraMemberSetting = null): void {
    this.jiraProjectDialogRef = this.dialog.open(JiraProjectProjectComponent);
    this.jiraProjectDialogRef.componentInstance.jiraSettingId = setting.jiraSettingId;
    this.jiraProjectDialogRef.componentInstance.jiraSetting = setting;
  }

  private loadJiraTable(){
    this.jiraSettingService.getJiraMemberSetting(this.authService.authUser.id).subscribe(result => {
      this.tableData = [],
      this.tableData = result;
    })
  }
}


