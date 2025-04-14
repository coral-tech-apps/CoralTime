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
import { debounceTime, Subject, switchMap } from "rxjs";
import { ROWS_ON_PAGE } from "src/app/core/constant.service";

@Component({
  selector: 'ct-jira-integration',
  templateUrl: 'jira-integration.component.html',
  standalone: false,
})

export class JiraIntegrationComponent {
  filterStr: string = '';
  @ViewChild('dt') tableRef!: Table;
  tableData: any[];
  pagedResult: PagedResult<JiraSetting>;
  updatingGrid: boolean = false;
  resizeObservable: Subject<any> = new Subject();
  isAllProjects: boolean = false;

  private subject = new Subject<any>();
  private lastEvent: any;
  private dialogRef: MatDialogRef<JiraIntegrationFormComponent>;
  private dialogUserRef: MatDialogRef<JiraUsersComponent>;


  constructor(private http: HttpClient,
    public authService: AuthService,
    private jiraSettingService: JiraSettingService,
    private notificationService: NotificationService,
    private dialog: MatDialog,){

}

ngOnInit(){
  this.loadInitialState();
}

private loadInitialState(): void{
  this.subject.pipe(debounceTime(500),switchMap(() => {
        return this.jiraSettingService.loadSettingsTable(this.authService.authUser.id, this.lastEvent, this.filterStr);
      }),)
        .subscribe((result : PagedResult<JiraSetting>) => {
          this.tableData = result.data;
          this.pagedResult = result;
          this.checkIsAllProjects();
        });
}

private checkIsAllProjects(): void {
  if (this.pagedResult && this.pagedResult.data.length >= this.pagedResult.count) {
    this.isAllProjects = true;
  }
}

openConnectionDialog(setting: JiraSetting = null): void {
  this.dialogRef = this.dialog.open(JiraIntegrationFormComponent);
  this.dialogRef.componentInstance.setting = setting;
  this.dialogRef.componentInstance.onSubmit.subscribe((response) => {
    this.onSubmit(response);
    this.dialogRef.close();
  });
}

openJiraUsersDialog(jiraSetting: JiraSetting): void {
  this.dialogUserRef = this.dialog.open(JiraUsersComponent, {
    panelClass: 'scrollable-dialog'
  });
  this.dialogUserRef.componentInstance.jiraSetting = jiraSetting;

  this.dialogUserRef.afterClosed().subscribe(result => {
    this.loadLazy(null, true);
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
  this.loadLazy(null, true);
}

filterTable(value: string): void{
  if(this.tableRef){
    this.tableRef.filterGlobal(value, 'contains');
  }
}

loadLazy(event = null, updatePage?: boolean): void {
    if (event) {
      this.lastEvent = event;
    }
    if (updatePage) {
      this.updatingGrid = updatePage;
      this.lastEvent.first = 0;
    }
    if (event || updatePage) {
      this.isAllProjects = false;
      this.pagedResult = null;
      this.resizeObservable.next(true);
    }
    this.lastEvent.rows = ROWS_ON_PAGE;
    if (!updatePage && this.isAllProjects) {
      return;
    }

    this.subject.next({
      event,
      filterStr: this.filterStr
    });
  }
}
