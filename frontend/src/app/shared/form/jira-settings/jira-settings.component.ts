import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { AclService } from "src/app/core/auth/acl.service";
import { AuthService } from "src/app/core/auth/auth.service";
import { ClientFormComponent } from "src/app/pages/clients/form/client-form.component";
import { JiraSettingFormComponent } from "./form/jira-settings-form.component";
import { JiraSetting } from "src/app/models/jira-setting";
import { JiraSettingService } from "src/app/services/jira-settings.service";
import { NotificationService } from "src/app/core/notification.service";
import { Subject, Subscriber } from 'rxjs';

@Component({
  selector: 'ct-jira-settings',
  templateUrl: 'jira-settings.component.html',
  standalone: false
})

export class JiraSettings implements OnInit{
showJiraTable: boolean;
tableData: any[] = [];

private dialogRef: MatDialogRef<JiraSettingFormComponent>;

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

  deleteSetting(index: number): void{
    this.jiraSettingService.deleteSetting(this.tableData[index].id).subscribe(result => {
      if(result){
        this.tableData.splice(index, 1);
        this.tableData = [...this.tableData];
      }
    })
  }

  openConnectionDialog(setting: JiraSetting = null): void {
    this.dialogRef = this.dialog.open(JiraSettingFormComponent);
    this.dialogRef.componentInstance.setting = setting;
    this.dialogRef.componentInstance.onSubmit.subscribe((response) => {
      this.dialogRef.close();
      this.onSubmit(response);
    });
  }

  private onSubmit(response: any): void {
		if (response.error) {
			this.notificationService.danger('Error saving jira setting.');
			return;
		}

		if (response.isNewSetting) {
			this.notificationService.success('New Jira setting has been successfully created.');
		} else {
			this.notificationService.success('New Jira setting has been successfully changed.');
		}
    this.loadInitialState();
	}

  private loadJiraTable(){
    this.jiraSettingService.loadSettingsTable(this.authService.authUser.id).subscribe(result => {
      this.tableData = [],
      this.tableData = result;
    })
  }
}


