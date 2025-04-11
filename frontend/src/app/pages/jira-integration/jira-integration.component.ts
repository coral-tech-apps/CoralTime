import { Component, ViewChild } from "@angular/core";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";
import { JiraIntegrationFormComponent } from "./form/jira-integration-form.component";
import { AuthService } from "src/app/core/auth/auth.service";
import { HttpClient } from "@angular/common/http";
import { NotificationService } from "src/app/core/notification.service";
import { JiraSetting } from "src/app/models/jira-setting";
import { JiraSettingService } from "src/app/services/jira-settings.service";
import { JiraUsersComponent } from "./jira-member-form/jira-member.component";
import { ConfirmDialogComponent } from "src/app/shared/form/confirm-dialog/confirm-dialog.component";
import { PagedResult } from "src/app/services/odata";
import { Table } from "primeng/table";

@Component({
  selector: 'ct-jira-integration',
  templateUrl: 'jira-integration.component.html',
  standalone: false,
})

export class JiraIntegrationComponent {
  filterStr: string = '';
  @ViewChild('dt') tableRef!: Table;
  tableData: any[];
  tableData1: PagedResult<JiraSetting>;

  private dialogRef: MatDialogRef<JiraIntegrationFormComponent>;
  private dialogUserRef: MatDialogRef<JiraUsersComponent>;


  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,
    private dialog: MatDialog,){

}

ngOnInit(){
  this.loadInitialState(this.filterStr);
}

//TODO: fix sorting
loadInitialState(filterStr: string): void{
  this.jiraSettingService.loadSettingsTable(this.authService.authUser.id, filterStr).subscribe((result : PagedResult<JiraSetting>) => {
    this.tableData = result.data;
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

openJiraUsersDialog(jiraSetting: JiraSetting): void {
  this.dialogUserRef = this.dialog.open(JiraUsersComponent);
  this.dialogUserRef.componentInstance.jiraSetting = jiraSetting;

  this.dialogUserRef.afterClosed().subscribe(result => {
    this.loadInitialState(this.filterStr);
  })
}

deleteSetting(index: number): void{
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Setting?',
        message: `Are you sure you want to delete "${this.tableData[index].settingName}"?`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.jiraSettingService.deleteSetting(this.tableData[index].id).subscribe(result => {
          if(result){
            this.tableData.splice(index, 1);
            this.tableData = [...this.tableData];
            this.notificationService.success('Jira setting successfuly deleted.');
          }else{
            this.notificationService.success('Error deleting jira setting.');
          }
        });
      }
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
    this.notificationService.success('Jira setting has been successfully changed.');
  }
  this.loadInitialState(this.filterStr);
}

filterTable(value: string): void{
  this.loadInitialState(value);
}
}
