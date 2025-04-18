import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { forkJoin as observableForkJoin, of as observableOf, Observable } from 'rxjs';
import { map, finalize } from 'rxjs/operators';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';
import { JiraMemberSetting } from 'src/app/models/jira-member-setting';

export class FormJiraSetting {
  id: number;
  settingName: string;
  userEmail: string;
  domain: string;
  apiToken: string;
  apiTokenStatus: boolean;

  static formJiraSetting(setting: JiraMemberSetting): FormJiraSetting {
    let instance = new this();
    instance.id = setting.id;
    instance.settingName = setting.settingName;
    instance.domain = setting.domain;
    instance.apiToken = setting.apiToken;
    instance.userEmail = setting.userEmail;
    instance.apiTokenStatus = setting.apiTokenStatus;
    return instance;
  }

  toSetting(setting: JiraMemberSetting): JiraMemberSetting {
    return new JiraMemberSetting({
      id: setting.id,
      settingName: this.settingName,
      userEmail: this.userEmail,
      domain: this.domain,
      apiToken: this.apiToken,
      apiTokenStatus: this.apiTokenStatus
    });
  }
}

@Component({
  selector: 'ct-jira-setting-form',
  templateUrl: 'jira-settings-form.component.html',
  standalone: false
})
export class JiraSettingFormComponent implements OnInit {
  @Input() setting: JiraMemberSetting;
  @Output() onSubmit = new EventEmitter();

  @ViewChild('email') emailControl: NgModel;
  @ViewChild('token') tokenControl: NgModel;

  isNewSetting: boolean;
  isRequestLoading: boolean;
  isValidateLoading: boolean;
  model: FormJiraSetting;
  dialogHeader: string;
  submitButtonText: string;
  showErrors: boolean[] = []; // [settingName, email, domain, token]
  dialogRef: any;

  constructor(private jiraSettingService: JiraSettingService) {}

  ngOnInit() {
    let setting = this.setting;
    this.isNewSetting = !setting;
    this.setting = setting ? setting : new JiraMemberSetting();
    this.submitButtonText = this.setting.id ? 'Save' : 'Create';
    this.dialogHeader = this.setting.id ? 'Edit' : 'Create New Jira Setting';
    this.model = FormJiraSetting.formJiraSetting(this.setting);
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

  private submit(form: NgForm): void {
    const updatedSetting = this.model.toSetting(this.setting);
    this.jiraSettingService.fillJiraMemberSetting(updatedSetting, this.setting.id).subscribe({
      next: (res) => {
        this.onSubmit.emit({ isNewSetting: false});
      }
    });
  }

  private validateForm(form: NgForm): Observable<boolean> {
    this.showErrors = [false, false, false, false];
    const isEmailValid = observableOf(form.controls['email'].valid);
    const isTokenValid = observableOf(form.controls['token'].valid);

    return observableForkJoin([isEmailValid, isTokenValid])
      .pipe(map((results: boolean[]) => {
        results.forEach((isValid, index) => this.showErrors[index] = !isValid);
        return results.every(valid => valid);
      }));
  }
}
