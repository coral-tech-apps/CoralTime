import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { JiraSettingService } from "./jira-settings.service";
import { AuthService } from "../core/auth/auth.service";

@Injectable({
  providedIn: 'root'
})
export class JiraEnableService {
  private showWorklogSubject = new BehaviorSubject<boolean>(false);
  showWorklog$ = this.showWorklogSubject.asObservable();

  constructor(
    private jiraSettingService: JiraSettingService,
    private authService: AuthService,
  ){
    this.authService.onChange.subscribe(authUser => {
      if(authUser){
        this.loadInitialState();
      } else{
        this.showWorklogSubject.next(false);
      }
    })
  }

  private loadInitialState(){
    this.jiraSettingService.isEnableJira().subscribe((value: boolean) => {
      this.showWorklogSubject.next(value);
    })
  }

  setShowWorklogs(value: boolean){
    this.showWorklogSubject.next(value);
  }
}
