import { Component } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { JiraIntegrationFormComponent } from "./form/jira-integration-form.component";
import { AuthService } from "src/app/core/auth/auth.service";
import { HttpClient } from "@angular/common/http";
import { NotificationService } from "src/app/core/notification.service";
import { JiraSetting } from "src/app/models/jira-setting";
import { JiraSettingService } from "src/app/services/jira-settings.service";
import { JiraUsersComponent } from "./jira-member-form/jira-member.component";

@Component({
  selector: 'ct-jira-integration',
  templateUrl: 'jira-integration.component.html',
  standalone: false,
})

export class JiraIntegrationComponent {
  private dialogRef: MatDialogRef<JiraIntegrationFormComponent>;
  private dialogUserRef: MatDialogRef<JiraUsersComponent>;
  tableData: any[] = [];

  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,
    private dialog: MatDialog,){

}

ngOnInit(){
  this.loadInitialState();
}

loadInitialState(): void{
  this.jiraSettingService.loadSettingsTable(this.authService.authUser.id).subscribe(result => {
    this.tableData = [],
    this.tableData = result;
  })
}

 openConnectionDialog(setting: JiraSetting = null): void {
    this.dialogRef = this.dialog.open(JiraIntegrationFormComponent);
    this.dialogRef.componentInstance.setting = setting;
    this.dialogRef.componentInstance.onSubmit.subscribe((response) => {
      this.dialogRef.close();
      this.onSubmit(response);
    });
  }

  openProjectUsersDialog(jiraSetting: JiraSetting): void {
    this.dialogUserRef = this.dialog.open(JiraUsersComponent);
    this.dialogUserRef.componentInstance.jiraSetting = jiraSetting;
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
}
