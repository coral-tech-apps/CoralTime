import { SharedModule } from '../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JiraIntegrationComponent } from './jira-integration.component';
import { JiraIntegrationRoutingModule } from './jira-integration-routin.module';
import { JiraUsersComponent } from './jira-member-form/jira-member.component';

@NgModule({
    imports: [
      CommonModule,
      SharedModule,
      JiraIntegrationRoutingModule
    ],
    declarations: [
      JiraIntegrationComponent,
      JiraUsersComponent
    ],
    exports: [
      JiraIntegrationComponent,
      JiraUsersComponent
    ]
})

export class JiraIntegrationModule{
}
