import { JiraSettingService } from 'src/app/services/jira-settings.service';
import { forkJoin as observableForkJoin, of as observableOf, Observable } from 'rxjs';
import { map, finalize } from 'rxjs/operators';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NgForm, NgModel } from '@angular/forms';
import { JiraSetting } from 'src/app/models/jira-setting';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export class FormJiraSetting {
  id: string;
  settingName: string;
  userEmail: string;
  domain: string;
  apiToken: string;

  static formJiraSetting(setting: JiraSetting): FormJiraSetting {
    let instance = new this();
    instance.id = setting.id;
    instance.settingName = setting.settingName;
    instance.userEmail = setting.userEmail;
    instance.domain = setting.domain;
    instance.apiToken = setting.apiToken;
    return instance;
  }

  toSetting(setting: JiraSetting): JiraSetting {
    return new JiraSetting({
      id: setting.id,
      settingName: this.settingName,
      userEmail: this.userEmail,
      domain: this.domain,
      apiToken: this.apiToken
    });
  }
}

@Component({
  selector: 'ct-jira-setting-form',
  templateUrl: 'jira-settings-form.component.html',
  imports: [CommonModule, FormsModule],

})
export class JiraSettingFormComponent implements OnInit {
  @Input() setting: JiraSetting;
  @Output() onSubmit = new EventEmitter();

  @ViewChild('email') emailControl: NgModel;
  @ViewChild('domain') domainControl: NgModel;
  @ViewChild('token') tokenControl: NgModel;
  @ViewChild('settingName') settingNameControl: NgModel;

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
    this.setting = setting ? setting : new JiraSetting();
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
    if(this.setting.id){
      this.jiraSettingService.updateSetting(updatedSetting, this.setting.id).subscribe({
        next: (res) => {
          console.log('updated ', res);
          this.onSubmit.emit({ isNewSetting: false});
        },
        error: (err) => {
          console.error('failed ', err);
        }
      });
    }else{
      this.jiraSettingService.createNewSetting(updatedSetting).subscribe({
        next: (res) => {
          this.onSubmit.emit({ isNewSetting: true});
        },
        error: (err) => {
          console.log('failed ', err);
        }
      });
    }
  }

  private validateForm(form: NgForm): Observable<boolean> {
    this.showErrors = [false, false, false, false];
    const isSettingNameValid = observableOf(form.controls['settingName'].valid);
    const isEmailValid = observableOf(form.controls['email'].valid);
    const isDomainValid = observableOf(form.controls['domain'].valid);
    const isTokenValid = observableOf(form.controls['token'].valid);

    return observableForkJoin([isSettingNameValid, isEmailValid, isDomainValid, isTokenValid])
      .pipe(map((results: boolean[]) => {
        results.forEach((isValid, index) => this.showErrors[index] = !isValid);
        return results.every(valid => valid);
      }));
  }
}
