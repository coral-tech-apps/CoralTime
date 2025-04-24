import { NotificationService } from 'src/app/core/notification.service';
import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { forkJoin as observableForkJoin, of as observableOf, Observable} from 'rxjs';
import { map, finalize} from 'rxjs/operators';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';
import { JiraSetting } from 'src/app/models/jira-setting';
import { Client } from 'src/app/models/client';
import { ClientsService } from 'src/app/services/clients.service';
import { JiraProjectService } from 'src/app/services/jira-project.service';
import { PagedResult } from 'src/app/services/odata';
import { AssignedJiraProject } from 'src/app/models/assigned-jira-project';

export class FormJiraSetting {
  id: number;
  settingName: string;
  domain: string;
  clientId: number;
  client: Client;

  static formJiraSetting(setting: JiraSetting): FormJiraSetting {
    let instance = new this();
    instance.id = setting.id;
    instance.settingName = setting.settingName;
    instance.domain = setting.domain;
    instance.clientId = setting.clientId;
    return instance;
  }

  toSetting(setting: JiraSetting): JiraSetting {
    return new JiraSetting({
      id: setting.id,
      settingName: this.settingName,
      domain: this.domain,
    });
  }
}

@Component({
  selector: 'ct-jira-integration-form',
  templateUrl: 'jira-integration-form.component.html',
  standalone: false
})
export class JiraIntegrationFormComponent implements OnInit {
  @Input() setting: JiraSetting;
  @Output() onSubmit = new EventEmitter();

  @ViewChild('domain') domainControl: NgModel;
  @ViewChild('settingName') settingNameControl: NgModel;

  isNewSetting: boolean;
  isRequestLoading: boolean;
  isValidateLoading: boolean;
  model: FormJiraSetting;
  dialogHeader: string;
  submitButtonText: string;
  showErrors: boolean[] = []; // [settingName, domain]
  dialogRef: any;
  clients: Client[];
  isProjectAssigned: boolean;


  private assignedProjectsLastEvent: any;
  private filterStr: string = '';

  constructor(private jiraSettingService: JiraSettingService,
              private clienService: ClientsService,
              private jiraProjectService: JiraProjectService,
              private notificationService: NotificationService,
  ) {}

  ngOnInit() {
    let setting = this.setting;
    this.isNewSetting = !setting;
    this.setting = setting ? setting : new JiraSetting();
    this.submitButtonText = this.setting.id ? 'Save' : 'Create';
    this.dialogHeader = this.setting.id ? 'Edit' : 'Create New Jira Setting';
    this.model = FormJiraSetting.formJiraSetting(this.setting);
    this.getClients();
    this.SetIsProjectAssigned();
  }

  validateAndSubmit(form: NgForm): void {
    this.isValidateLoading = true;
    this.validateForm(form).pipe(
      finalize(() => this.isValidateLoading = false)
    ).subscribe((isFormValid: boolean) => {
      if (isFormValid) {
        this.submit(form);
      }
    });
  }

  private getClients(): void{
    this.clienService.getClients().subscribe(result => {
      this.clients = result;
    })
  }

  private submit(form: NgForm): void {
    const updatedSetting = this.model.toSetting(this.setting);
    updatedSetting.clientId = this.model?.client?.id ?? this.model.clientId;

    if(this.setting.id){
      this.jiraSettingService.updateSetting(updatedSetting, this.setting.id).subscribe({
        next: (res) => {
          this.onSubmit.emit({ isNewSetting: false});
          this.notificationService.success("Successfully update Jira setting");
        },
        error: (err) => {
          this.notificationService.danger("Erro while updation Jira setting");
        }
      });
    }else{
      this.jiraSettingService.createNewSetting(updatedSetting).subscribe({
        next: (res) => {
          this.onSubmit.emit({isNewSetting: true});
          this.notificationService.success("Successfully create Jira setting");
        },
        error: (err) => {
          this.notificationService.danger("Erro while creating Jira setting");
        }
      });
    }
  }

  private validateForm(form: NgForm): Observable<boolean> {
    this.showErrors = [false, false, false, false];
    const isSettingNameValid = observableOf(form.controls['settingName'].valid);
    const isClientIsValid = observableOf(!!this.model?.client?.id || !!this.model?.clientId);
    const isDomainValid = observableOf(form.controls['domain'].valid);

    return observableForkJoin([isSettingNameValid, isClientIsValid, isDomainValid])
      .pipe(map((results: boolean[]) => {
        results.forEach((isValid, index) => this.showErrors[index] = !isValid);

        return results.every(valid => valid);
      }));
  }

  get isChangeableLocker(): boolean{
    return this.isProjectAssigned;
  }

  private SetIsProjectAssigned(): void{
    if(!this.assignedProjectsLastEvent){
      this.assignedProjectsLastEvent = {
        first: 0,
        rows: 12
      }
    }

    this.jiraProjectService.getAssignedProjects(this.setting.id, this.assignedProjectsLastEvent, this.filterStr).subscribe((result: PagedResult<AssignedJiraProject>) => {
      if(result.data.length > 0){
        this.isProjectAssigned = true;
      }else{
        this.isProjectAssigned = false;
      }
    })
  }
}
